using Plugin.InAppBilling;

namespace TravelBlog.Models
{
    public class PurchaseDBModelResponse
    {
        public int Id { get; set; }


        public string ProductId { get; set; }


        public ItemType PurchaseType { get; set; }

        public IEnumerable<PurchaseDBModelDetailResponse> Items { get; set; }
    }

    public class PurchaseDBModelDetailResponse
    {
        public int Id { get; set; }

        public int PurchaseModelId { get; set; }

        public string PurchaseId { get; set; }

        public bool AutoRenewing { get; set; }

        public bool? IsAcknowledged { get; set; }

        public DateTime TransactionDateUtc { get; set; }
    }
}