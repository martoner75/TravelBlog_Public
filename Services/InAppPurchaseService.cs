using Microsoft.Extensions.Logging;
using Plugin.InAppBilling;
using System.Text.Json;
using TravelBlog.Models;

namespace TravelBlog.Services
{
    public class InAppPurchaseService : IInAppPurchaseService
    {
        private readonly Settings _settings;
        private readonly ILogger<InAppPurchaseService> _logger;
        private readonly IInAppBilling _billing;

        public InAppPurchaseService(
            ILogger<InAppPurchaseService> logger,
            Settings settings)
        {
            _settings = settings;
            _logger = logger;
            _billing = CrossInAppBilling.Current;
        }

        public async Task<IEnumerable<PurchaseModel>?> GetAllPurchasesAsync()
        {
            var purchaseResult = new List<PurchaseModel>();

            try
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: Loading purchases");

                var connected = await _billing.ConnectAsync();

                if (!connected)
                    purchaseResult.Add(new PurchaseModel(string.Empty, ItemType.Subscription) { Errors = [$"There was an error while connecting to the store"] });

                var subscriptions = await _billing.GetPurchasesAsync(ItemType.Subscription);
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: Subscriptions loaded: {JsonSerializer.Serialize(subscriptions)}");

                var inAppPurchaseConsumable = await _billing.GetPurchasesAsync(ItemType.InAppPurchaseConsumable);
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: IAP Consumable loaded: {JsonSerializer.Serialize(inAppPurchaseConsumable)}");

                var inAppPurchase = await _billing.GetPurchasesAsync(ItemType.InAppPurchase);
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: IAP loaded: {JsonSerializer.Serialize(inAppPurchase)}");

                purchaseResult.Add(new PurchaseModel(_settings.Subscription, ItemType.Subscription) { PurchaseItems = subscriptions });
                purchaseResult.Add(new PurchaseModel(_settings.SubscriptionNR, ItemType.Subscription) { PurchaseItems = subscriptions });
                purchaseResult.Add(new PurchaseModel(_settings.ConsumableIAP, ItemType.InAppPurchaseConsumable) { PurchaseItems = inAppPurchaseConsumable });
                purchaseResult.Add(new PurchaseModel(_settings.NonConsumableIAP, ItemType.InAppPurchaseConsumable) { PurchaseItems = inAppPurchase });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: Error while loading purchases {ex.Message}: {ex.StackTrace}");
                purchaseResult.Add(new PurchaseModel(string.Empty, ItemType.Subscription) { Errors = new List<string> { $"There was an error while retrieving purchases: {ex.Message} {ex.StackTrace}" } });
            }
            finally
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: Purchases loaded successfully");
                await _billing.DisconnectAsync();
            }

            return purchaseResult;
        }

        public async Task<IEnumerable<PurchaseModel>> GetPurchaseByProductIdAsync(
            ItemType type,
            string productId)
        {
            var purchaseResult = new List<PurchaseModel>();

            try
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetPurchaseByProductIdAsync)}: Loading purchase by Product Id");

                var connected = await _billing.ConnectAsync();

                if (!connected)
                    purchaseResult.Add(new PurchaseModel(productId, type) { Errors = new List<string> { $"There was an error while connecting to the store" } });

                purchaseResult.Add(new PurchaseModel(productId, type) { PurchaseItems = await _billing.GetPurchasesAsync(type) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(InAppPurchaseService)} > {nameof(GetPurchaseByProductIdAsync)}: Error while loading purchase: {ex.Message} {ex.StackTrace}");
                purchaseResult.Add(new PurchaseModel(productId, type) { Errors = new List<string> { $"There was an error while retrieving purchases: {ex.Message} {ex.StackTrace}" } });
            }
            finally
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetPurchaseByProductIdAsync)}: Purchase loaded successfully");
                await _billing.DisconnectAsync();
            }

            return purchaseResult;
        }

        public async Task<PurchaseModel?> PurchaseAsync(
            ItemType type,
            string productId)
        {
            PurchaseModel purchaseResult = new(productId, type);

            try
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Purchasing {productId}: {type}");

                var connected = await _billing.ConnectAsync();

                if (!connected)
                    purchaseResult.Errors.ToList().Add($"There was an error while connecting to the store");

                var productInfo = (await _billing.GetProductInfoAsync(type, productId)).FirstOrDefault();
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Loaded info for {productInfo}");

                var purchase = await _billing.PurchaseAsync(productInfo.ProductId, type);
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Purchase result: {purchase}");

                purchaseResult.purchaseState = purchase.State;

                if (purchase == null)
                    purchaseResult.Errors.ToList().Add("There was an error while purchasing this product");
                else if (purchase.State == PurchaseState.Purchased)
                {
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Finalisying purchase");
                    var ack = await CrossInAppBilling.Current.FinalizePurchaseAsync(purchase.TransactionIdentifier);

                    foreach (var item in ack)
                    {
                        _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Acknowledging purchase for {item.Id}");
                        purchaseResult.Acknowledgements.ToList().Add(new Acknowledgement() { ProductId = item.Id, Success = item.Success });
                    }
                }

                purchaseResult.PurchaseItems = await _billing.GetPurchasesAsync(type);
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                _logger.LogError($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Error while purchasing product: {purchaseEx.Message} {purchaseEx.StackTrace}");
                purchaseResult.Errors.ToList().Add($"There was an error while purchasing this product: {purchaseEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Error while purchasing product: {ex.Message} {ex.StackTrace}");
                purchaseResult.Errors.ToList().Add($"There was an error while purchasing this product: {ex.Message} {ex.StackTrace}");
            }
            finally
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Purchase completed successfully");
                await _billing.DisconnectAsync();
            }

            return purchaseResult;
        }
    }
}