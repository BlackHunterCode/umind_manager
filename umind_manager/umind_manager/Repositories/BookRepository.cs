using Dapper;
using System.Data;
using umind_manager.Core.Contracts.Repositories;
using umind_manager.Core.Entities;
using Umind_Manager.Repositories.Dependencies;

namespace umind_manager.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly IDbConnection _connection;

        public BookRepository(DataDependency data)
        {
            _connection = data.Connection;
        }

        public async Task<int> InsertAsync(Books book)
        {
            var sql = @"
                INSERT INTO Books (Title, Sumary, PathPdf, PathCape, Active)
                VALUES (@Title, @Sumary, @PathPdf, @PathCape, @Active);
                SELECT last_insert_rowid();";

            return await _connection.ExecuteScalarAsync<int>(sql, book);
        }

        public async Task<Books?> GetByIdAsync(int id)
        {
            return await _connection.QueryFirstOrDefaultAsync<Books>(
                "SELECT * FROM Books WHERE Id = @id", new { id });
        }

        public async Task<int> UpdateBookAsync(Books book)
        {
            var sql = @"
                        UPDATE Books
                        SET Title = @Title,
                            Sumary = @Sumary,
                            PathPdf = @PathPdf,
                            PathCape = @PathCape,
                            Active = @Active,
                            UpdateAt = CURRENT_TIMESTAMP
                        WHERE Id = @Id;";

            return await _connection.ExecuteAsync(sql, book);
        }

        public async Task<int> DeleteBookAsync(int id)
        {
            var sql = @"
                        DELETE FROM Books
                        WHERE Id = @Id;";

            return await _connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<Books>> GetAllAsync()
        {
            return await _connection.QueryAsync<Books>("SELECT * FROM Books");
        }
    }
}
