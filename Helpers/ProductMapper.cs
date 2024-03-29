using TravelBlog.Models;

namespace TravelBlog.Helpers
{
    public static class ProductMapper
    {
        public static IEnumerable<PurchaseDBModelResponse> GetProductsFromModel(
            IEnumerable<PurchaseDBModel> items,
            IEnumerable<PurchaseDBModelDetail> itemDetails)
        {
            var result = new List<PurchaseDBModelResponse>();

            foreach (var item in items)
            {
                result.Add(new PurchaseDBModelResponse()
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    PurchaseType = item.PurchaseType,
                    Items = GetProductDetailsFromModel(itemDetails.Where(x=>x.PurchaseModelId == item.Id))
                });
            }

            return result;
        }

        private static IEnumerable<PurchaseDBModelDetailResponse> GetProductDetailsFromModel(IEnumerable<PurchaseDBModelDetail> items)
        {
            var result = new List<PurchaseDBModelDetailResponse>();

            foreach (var item in items)
            {
                result.Add(new PurchaseDBModelDetailResponse()
                {
                    Id = item.Id,
                    AutoRenewing = item.AutoRenewing,
                    IsAcknowledged = item.IsAcknowledged,
                    PurchaseModelId = item.PurchaseModelId,
                    PurchaseId = item.PurchaseId,
                    TransactionDateUtc = item.TransactionDateUtc
                });
            }

            return result;
        }
    }
}