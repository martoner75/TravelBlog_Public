using Plugin.InAppBilling;
using TravelBlog.Models;

namespace TravelBlog.Services
{
    public interface IInAppPurchaseService
    {
        Task<IEnumerable<PurchaseModel>> GetAllPurchasesAsync();

        Task PurchaseAsync(ItemType type, string productId);
    }
}