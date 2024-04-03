using Microsoft.Extensions.Logging;
using Plugin.InAppBilling;
using System.Text.Json;
using TravelBlog.Models;

namespace TravelBlog.Services
{
    public class InAppPurchaseService(
        ILogger<InAppPurchaseService> logger,
        Settings settings) : IInAppPurchaseService
    {
        private readonly Settings _settings = settings;
        private readonly ILogger<InAppPurchaseService> _logger = logger;
        private readonly IInAppBilling _billing = CrossInAppBilling.Current;

        public async Task<IEnumerable<PurchaseModel>?> GetAllPurchasesAsync()
        {
            var purchaseResult = new List<PurchaseModel>();

            try
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: Loading purchases");

                var connected = await _billing.ConnectAsync();

                if (!connected)
                    purchaseResult.Add(new PurchaseModel(string.Empty, ItemType.Subscription) { Errors = [$"There was an error while connecting to the store"] });
                else
                {
                    var subscription = await _billing.GetProductInfoAsync(ItemType.Subscription, "TravelBlog_S", "tb_nr_s");
                    var iapurchaseC = await _billing.GetProductInfoAsync(ItemType.InAppPurchaseConsumable, "TravelBlog");
                    var iapurchaseNC = await _billing.GetProductInfoAsync(ItemType.InAppPurchase, "TravelBlog_c_iap");

                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: All subscriptions products: {JsonSerializer.Serialize(subscription)}");
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: All IAPc products: {JsonSerializer.Serialize(iapurchaseC)}");
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: All IAPnc products: {JsonSerializer.Serialize(iapurchaseNC)}");

                    var subscriptions = (await _billing.GetPurchasesAsync(ItemType.Subscription));
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: All subscriptions: {JsonSerializer.Serialize(subscriptions)}");

                    var subscriptionsR = subscriptions.Where(x => x.ProductId.Equals(_settings.Subscription));
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: Subscriptions R loaded: {JsonSerializer.Serialize(subscriptionsR)}");

                    var subscriptionsNR = subscriptions.Where(x => x.ProductId.Equals(_settings.SubscriptionNR));
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: Subscriptions NR loaded: {JsonSerializer.Serialize(subscriptionsNR)}");

                    var inAppPurchaseConsumable = (await _billing.GetPurchasesAsync(ItemType.InAppPurchaseConsumable));
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: IAP Consumable loaded: {JsonSerializer.Serialize(inAppPurchaseConsumable)}");

                    var inAppPurchase = (await _billing.GetPurchasesAsync(ItemType.InAppPurchase));
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: IAP loaded: {JsonSerializer.Serialize(inAppPurchase)}");

                    purchaseResult.Add(new PurchaseModel(_settings.Subscription, ItemType.Subscription) { PurchaseItems = subscriptionsR });
                    purchaseResult.Add(new PurchaseModel(_settings.SubscriptionNR, ItemType.Subscription) { PurchaseItems = subscriptionsNR });
                    purchaseResult.Add(new PurchaseModel(_settings.ConsumableIAP, ItemType.InAppPurchaseConsumable) { PurchaseItems = inAppPurchaseConsumable });
                    purchaseResult.Add(new PurchaseModel(_settings.NonConsumableIAP, ItemType.InAppPurchase) { PurchaseItems = inAppPurchase });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: Error while loading purchases {ex.Message}: {ex.StackTrace}");
            }
            finally
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(GetAllPurchasesAsync)}: Purchases loaded successfully");
                await _billing.DisconnectAsync();
            }

            return purchaseResult;
        }

        public async Task PurchaseAsync(
            ItemType type,
            string productId)
        {
            try
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Purchasing {productId}: {type}");

                var connected = await _billing.ConnectAsync();

                if (!connected)
                {
                    _logger.LogError($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Error while purchasing connecting to the store");

                    return;
                }

                var productInfo = (await _billing.GetProductInfoAsync(type, productId)).FirstOrDefault();

                if((await _billing.GetProductInfoAsync(type, productId)).FirstOrDefault() != null)
                {
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Loaded info for {productInfo.ProductId}");

                    var purchase = await _billing.PurchaseAsync(productInfo.ProductId, type);
                    
                    _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Purchase result: {purchase}");

                    if (purchase.State == PurchaseState.Purchased)
                    {
                        _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Finalisying purchase");
                        var ack = await CrossInAppBilling.Current.FinalizePurchaseAsync(purchase.TransactionIdentifier);

                        foreach (var item in ack)
                            _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Acknowledging purchase for {item.Id} was {item.Success}");
                    }
                    else
                        _logger.LogWarning($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Purchase state for {productInfo.ProductId}/{type} was {purchase.State}");
                }
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                _logger.LogError($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Error while purchasing product {productId}/{type}: {purchaseEx.Message} {purchaseEx.StackTrace}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Error while purchasing product {productId}/{type}: {ex.Message} {ex.StackTrace}");
            }
            finally
            {
                _logger.LogInformation($"{nameof(InAppPurchaseService)} > {nameof(PurchaseAsync)}: Purchase {productId}/{type} completed successfully");

                await _billing.DisconnectAsync();
            }
        }
    }
}