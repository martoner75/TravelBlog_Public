using TravelBlog.Models;

namespace TravelBlog.Services
{
    public interface IBaseRepository
    {
        Task<List<PurchaseDBModel>> GetItemsAsync();

        Task<PurchaseDBModel> GetItemAsync(string id);

        Task<List<PurchaseDBModelDetail>> GetItemsDetailsAsync();

        Task<PurchaseDBModelDetail> GetItemDetailsAsync(int productDetailId);

        Task<IEnumerable<PurchaseDBModelDetail>> GetItemDetailsByProductIdAsync(int productId);

        Task<int> SaveItemAsync(PurchaseDBModel item);

        Task<int> SaveItemDetailAsync(PurchaseDBModelDetail item);

        Task<int> DeleteItemAsync(PurchaseDBModel item);
    }
}