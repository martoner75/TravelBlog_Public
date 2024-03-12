using Plugin.InAppBilling;
using TravelBlog.Models;

namespace TravelBlog.Services
{
    public interface IInAppPurchaseService
    {
        Task<IEnumerable<PurchaseModel>> GetAllPurchasesAsync();

        Task<IEnumerable<PurchaseModel>> GetPurchaseByProductIdAsync(ItemType type, string productId);

        Task<PurchaseModel> PurchaseAsync(ItemType type, string productId);
    }
}