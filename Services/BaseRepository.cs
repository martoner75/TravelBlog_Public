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
                await _repository.CreateTableAsync<PurchaseDBModelDetail>();
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

        public async Task<PurchaseDBModelDetail> GetItemDetailsAsync(int productDetailId)
        {
            await Init();

            return await _repository.Table<PurchaseDBModelDetail>().Where(i => i.Id == productDetailId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PurchaseDBModelDetail>> GetItemDetailsByProductIdAsync(int productId)
        {
            await Init();

            return await _repository.Table<PurchaseDBModelDetail>().Where(i => i.PurchaseModelId == productId).ToListAsync();
        }

        public async Task<int> SaveItemAsync(PurchaseDBModel item)
        {
            await Init();

            try
            {
                if(await GetItemAsync(item.ProductId) != null)
                    await _repository.DeleteAsync(item);

                await _repository.InsertAsync(item);

                return (await GetItemAsync(item.ProductId)).Id;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<int> SaveItemDetailAsync(PurchaseDBModelDetail item)
        {
            await Init();

            try
            {
                if (await GetItemDetailsAsync(item.Id) != null)
                    await _repository.DeleteAsync(item);

                await _repository.InsertAsync(item);

                return (await GetItemDetailsAsync(item.Id)).Id;
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

        public async Task<List<PurchaseDBModelDetail>> GetItemsDetailsAsync()
        {
            try
            {
                await Init();

                return await _repository.Table<PurchaseDBModelDetail>().ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<PurchaseDBModelDetail>();
            }
        }
    }
}