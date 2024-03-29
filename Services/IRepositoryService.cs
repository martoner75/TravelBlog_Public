using TravelBlog.Models;

namespace TravelBlog.Services
{
    public interface IRepositoryService
    {
        Task StorePurchases(IEnumerable<PurchaseModel> purchasesFromStore);

        Task<IEnumerable<PurchaseDBModelResponse>> GetStorePurchases();
    }
}