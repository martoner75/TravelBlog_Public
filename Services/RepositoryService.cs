using TravelBlog.Helpers;
using TravelBlog.Models;

namespace TravelBlog.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly IBaseRepository _baseRepository;

        public RepositoryService(IBaseRepository baseRepository) 
        {
            _baseRepository = baseRepository;
        }

        public async Task StorePurchases(IEnumerable<PurchaseModel> purchasesFromStore)
        {
            foreach (var purchase in purchasesFromStore)
            {
                var productId = await _baseRepository.SaveItemAsync(
                    new PurchaseDBModel()
                    {
                        ProductId = purchase._productId,
                        PurchaseType = purchase._purchaseType
                    });

                foreach (var purchaseDetail in purchase.PurchaseItems)
                {
                    var itemAdded = await _baseRepository.SaveItemDetailAsync(
                        new PurchaseDBModelDetail()
                        {
                            PurchaseModelId = productId,
                            PurchaseId = purchaseDetail.Id,
                            IsAcknowledged = purchaseDetail.IsAcknowledged != null ? purchaseDetail.IsAcknowledged : false,
                            TransactionDateUtc = purchaseDetail.TransactionDateUtc,
                            AutoRenewing = purchaseDetail.AutoRenewing
                        });
                }
            }
        }

        public async Task<IEnumerable<PurchaseDBModelResponse>> GetStorePurchases()
        {
            var result = new List<PurchaseDBModelResponse>();
            var storedPurchases = await _baseRepository.GetItemsAsync();
            var storedPurchaseDetailss = await _baseRepository.GetItemsDetailsAsync();

            result.AddRange(
                ProductMapper.GetProductsFromModel(
                    storedPurchases,
                    storedPurchaseDetailss)
                );

            return result;
        }
    }
}