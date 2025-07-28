using umind_manager.Core.Entities;

namespace umind_manager.Core.Contracts.Repositories
{
    public interface IBookRepository
    {
        Task<int> InsertAsync(Books book);
        Task<Books?> GetByIdAsync(int id);
        Task<IEnumerable<Books>> GetAllAsync();
        Task<int> UpdateBookAsync(Books book);
        Task<int> DeleteBookAsync(int id);
    }
}
