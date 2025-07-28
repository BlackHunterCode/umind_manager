using umind_manager.Core.Entities;

namespace umind_manager.Core.Contracts.Repositories
{
    public interface IMissionRepository
    {
        Task<int> InsertAsync(Mission mission);
        Task<Mission?> GetByIdAsync(int id);
        Task<IEnumerable<Mission>> GetByBookIdAsync(int bookId);
    }
}
