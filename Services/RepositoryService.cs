using Microsoft.Extensions.Logging;
using TravelBlog.Helpers;
using TravelBlog.Models;

namespace TravelBlog.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly IBaseRepository _baseRepository;
        private readonly ILogger<RepositoryService> _logger;

        public RepositoryService(
            IBaseRepository baseRepository,
            ILogger<RepositoryService> logger)
        {
            _baseRepository = baseRepository;
            _logger = logger;
        }

        public async Task StorePurchases(IEnumerable<PurchaseModel> purchasesFromStore)
        {
            _logger.LogInformation($"{nameof(RepositoryService)} > {nameof(StorePurchases)}: Storing purchase on the local archive");

            try
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
            catch(Exception ex)
            {
                _logger.LogInformation($"{nameof(RepositoryService)} > {nameof(StorePurchases)}: Error while storing purchases on the local archive {ex.Message} {ex.StackTrace}");
            }
        }

        public async Task<IEnumerable<PurchaseDBModelResponse>> GetStoredPurchases()
        {
            var result = new List<PurchaseDBModelResponse>();

            try
            {
                _logger.LogInformation($"{nameof(RepositoryService)} > {nameof(GetStoredPurchases)}: Retriving purchases from the local archive");

                var storedPurchases = await _baseRepository.GetItemsAsync();
                var storedPurchaseDetailss = await _baseRepository.GetItemsDetailsAsync();

                result.AddRange(
                    ProductMapper.GetProductsFromModel(
                        storedPurchases,
                        storedPurchaseDetailss)
                    );
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{nameof(RepositoryService)} > {nameof(GetStoredPurchases)}: Error while retriving purchases from the local archive {ex.Message} {ex.StackTrace}");
            }

            return result;
        }
    }
}