using Plugin.InAppBilling;

namespace TravelBlog.Models
{
    public class PurchaseModel
    {
        public PurchaseModel(
            string ProductId,
            ItemType PurchaseType)
        {
            _productId = ProductId;
            _purchaseType = PurchaseType;
        }

        public string _productId { get; }

        public ItemType _purchaseType { get; }

        public PurchaseState purchaseState { get; set; }

        public IEnumerable<string> Errors { get; set; } = new List<string>();

        public IEnumerable<InAppBillingPurchase> PurchaseItems { get; set; } = new List<InAppBillingPurchase>();

        public IEnumerable<Acknowledgement> Acknowledgements { get; set; } = new List<Acknowledgement>();
    }

    public class Acknowledgement
    {
        public string ProductId { get; set; }

        public bool Success { get; set; }
    }
}