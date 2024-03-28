using TravelBlog.Models;

namespace TravelBlog.Services
{
    public interface IBaseRepository
    {
        Task<List<PurchaseDBModel>> GetItemsAsync();

        Task<PurchaseDBModel> GetItemAsync(string id);

        Task<int> SaveItemAsync(PurchaseDBModel item);

        Task<int> DeleteItemAsync(PurchaseDBModel item);
    }
}