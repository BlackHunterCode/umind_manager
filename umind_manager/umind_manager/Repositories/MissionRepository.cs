using Dapper;
using System.Data;
using umind_manager.Core.Contracts.Repositories;
using umind_manager.Core.Entities;
using Umind_Manager.Repositories.Dependencies;

namespace umind_manager.Repositories
{
    public class MissionRepository : IMissionRepository
    {
        private readonly IDbConnection _connection;

        public MissionRepository(DataDependency data)
        {
            _connection = data.Connection;
        }

        public async Task<int> InsertAsync(Mission mission)
        {
            var sql = @"
                INSERT INTO Mission (BookId, Title, Description, Level)
                VALUES (@BookId, @Title, @Description, @Level);
                SELECT last_insert_rowid();";

            return await _connection.ExecuteScalarAsync<int>(sql, mission);
        }

        public async Task<Mission?> GetByIdAsync(int id)
        {
            return await _connection.QueryFirstOrDefaultAsync<Mission>(
                "SELECT * FROM Mission WHERE Id = @id", new { id });
        }

        public async Task<IEnumerable<Mission>> GetByBookIdAsync(int bookId)
        {
            return await _connection.QueryAsync<Mission>(
                "SELECT * FROM Mission WHERE BookId = @bookId", new { bookId });
        }
    }
}
