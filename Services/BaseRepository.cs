using SQLite;
using TravelBlog.Models;
using TravelBlog.Helpers;

namespace TravelBlog.Services
{
    public class BaseRepository: IBaseRepository
    {
        private SQLiteAsyncConnection _repository;

        async Task Init()
        {
            if (_repository is not null)
                return;

            _repository = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

            try
            {
                await _repository.CreateTableAsync<PurchaseDBModel>();
            }
            catch (Exception ex)
            {
                await Task.FromResult(ex);
            }
        }

        public async Task<List<PurchaseDBModel>> GetItemsAsync()
        {
            try
            {
                await Init();

                return await _repository.Table<PurchaseDBModel>().ToListAsync();
            }
            catch (Exception ex) { 
                return new List<PurchaseDBModel>();
            }
        }

        public async Task<PurchaseDBModel> GetItemAsync(string productId)
        {
            await Init();

            return await _repository.Table<PurchaseDBModel>().Where(i => i.ProductId == productId).FirstOrDefaultAsync();
        }

        public async Task<int> SaveItemAsync(PurchaseDBModel item)
        {
            await Init();

            try
            {
                if(await GetItemAsync(item.ProductId) != null)
                    return await _repository.UpdateAsync(item);
                else
                    return await _repository.InsertAsync(item);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<int> DeleteItemAsync(PurchaseDBModel item)
        {
            await Init();

            return await _repository.DeleteAsync(item);
        }
    }
}