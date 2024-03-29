using Plugin.InAppBilling;
using TravelBlog.Models;

namespace TravelBlog.Services
{
    public class InAppPurchaseService : IInAppPurchaseService
    {
        private readonly Settings _settings;
        private readonly IInAppBilling _billing;

        public InAppPurchaseService(Settings settings)
        {
            _settings = settings;
            _billing = CrossInAppBilling.Current;
        }

        public async Task<IEnumerable<PurchaseModel>?> GetAllPurchasesAsync()
        {
            var purchaseResult = new List<PurchaseModel>();

            try
            {
                var connected = await _billing.ConnectAsync();

                if (!connected)
                    purchaseResult.Add(new PurchaseModel(string.Empty, ItemType.Subscription) { Errors = new List<string> { $"There was an error while connecting to the store" } });

                purchaseResult.Add(new PurchaseModel(_settings.Subscription, ItemType.Subscription) { PurchaseItems = await _billing.GetPurchasesAsync(ItemType.Subscription) });
                purchaseResult.Add(new PurchaseModel(_settings.SubscriptionNR, ItemType.Subscription) { PurchaseItems = await _billing.GetPurchasesAsync(ItemType.Subscription) });
                purchaseResult.Add(new PurchaseModel(_settings.ConsumableIAP, ItemType.InAppPurchaseConsumable) { PurchaseItems = await _billing.GetPurchasesAsync(ItemType.InAppPurchaseConsumable) });
                purchaseResult.Add(new PurchaseModel(_settings.NonConsumableIAP, ItemType.InAppPurchaseConsumable) { PurchaseItems = await _billing.GetPurchasesAsync(ItemType.InAppPurchase) });
            }
            catch (Exception ex)
            {
                purchaseResult.Add(new PurchaseModel(string.Empty, ItemType.Subscription) { Errors = new List<string> { $"There was an error while retrieving purchases: {ex.Message} {ex.StackTrace}" } });
            }
            finally
            {
                await _billing.DisconnectAsync();
            }

            return purchaseResult.Where(x => x.PurchaseItems.Any(y => y.State == PurchaseState.Purchased));
        }

        public async Task<IEnumerable<PurchaseModel>> GetPurchaseByProductIdAsync(
            ItemType type,
            string productId)
        {
            var purchaseResult = new List<PurchaseModel>();

            try
            {
                var connected = await _billing.ConnectAsync();

                if (!connected)
                    purchaseResult.Add(new PurchaseModel(productId, type) { Errors = new List<string> { $"There was an error while connecting to the store" } });

                purchaseResult.Add(new PurchaseModel(productId, type) { PurchaseItems = await _billing.GetPurchasesAsync(type) });
            }
            catch (Exception ex)
            {
                purchaseResult.Add(new PurchaseModel(productId, type) { Errors = new List<string> { $"There was an error while retrieving purchases: {ex.Message} {ex.StackTrace}" } });
            }
            finally
            {
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
                var connected = await _billing.ConnectAsync();

                if (!connected)
                    purchaseResult.Errors.ToList().Add($"There was an error while connecting to the store");

                var productInfo = await _billing.GetProductInfoAsync(type, productId);
                var purchase = await _billing.PurchaseAsync(productId, type);
                purchaseResult.purchaseState = purchase.State;

                if (purchase == null)
                    purchaseResult.Errors.ToList().Add("There was an error while purchasing this product");
                else if (purchase.State == PurchaseState.Purchased)
                {
                    var ack = await CrossInAppBilling.Current.FinalizePurchaseAsync(purchase.TransactionIdentifier);

                    foreach (var item in ack)
                        purchaseResult.Acknowledgements.ToList().Add(new Acknowledgement() { ProductId = item.Id, Success = item.Success });
                }

                purchaseResult.PurchaseItems = await _billing.GetPurchasesAsync(type);
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                purchaseResult.Errors.ToList().Add($"There was an error while purchasing this product: {purchaseEx.Message}");
            }
            catch (Exception ex)
            {
                purchaseResult.Errors.ToList().Add($"There was an error while purchasing this product: {ex.Message} {ex.StackTrace}");
            }
            finally
            {
                await _billing.DisconnectAsync();
            }

            return purchaseResult;
        }
    }
}