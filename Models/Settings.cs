namespace TravelBlog.Models
{
    public class Settings
    {
        public string Domain { get; set; }

        public string ClientId { get; set; }

        public string RedirectUri { get; set; }

        public string PostLogoutRedirectUri { get; set; }

        public string Scope { get; set; }

        public string NonConsumableIAP { get; set; }

        public string ConsumableIAP { get; set; }

        public string Subscription { get; set; }

        public string SubscriptionNR { get; set; }

        public string FreeProductId { get; set; }

        public string FreeProductName { get; set; }
    }
}