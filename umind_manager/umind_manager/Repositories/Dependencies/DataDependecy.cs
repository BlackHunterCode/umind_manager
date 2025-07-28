using Microsoft.Data.Sqlite;
using System.Data;
using umind_manager.Core.Contracts.Repositories;
using umind_manager.Repositories;

namespace Umind_Manager.Repositories.Dependencies
{
    public class DataDependency : IDisposable
    {
        public IDbConnection Connection { get; }

        private bool _initialized;
        private readonly SqliteConnection _sqliteConnection;

        public DataDependency()
        {
            _sqliteConnection = new SqliteConnection("Data Source=:memory:");
            _sqliteConnection.Open();
            Connection = _sqliteConnection;

            InitializeSchema();
        }

        private void InitializeSchema()
        {
            if (_initialized) return;

            var command = Connection.CreateCommand();
            command.CommandText = @"
                                    CREATE TABLE Books (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        Title TEXT NOT NULL,
                                        Sumary TEXT,
                                        PathPdf TEXT,
                                        PathCape TEXT,
                                        Active BOOLEAN NOT NULL DEFAULT 1,
                                        CreateAt DATETIME NOT NULL,
                                        UpdateAt DATETIME NOT NULL
                                    );

                                    CREATE TABLE Mission (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        BookId INTEGER NOT NULL,
                                        Title TEXT NOT NULL,
                                        Description TEXT,
                                        Level INTEGER,
                                        FOREIGN KEY (BookId) REFERENCES Books(Id)
                                    );
                                ";
            command.ExecuteNonQuery();

            // Inserir dados de teste
            var insertCmd = Connection.CreateCommand();
            insertCmd.CommandText = @"
                                        INSERT INTO Books (Title, Sumary, PathPdf, PathCape, Active, CreateAt, UpdateAt) VALUES
                                        ('O Poder do Hábito', 'Livro sobre mudança de hábitos', 'poder_do_habito.pdf', 'capa_habito.jpg', 1, datetime('now'), datetime('now')),
                                        ('Mindset', 'Psicologia do sucesso', 'mindset.pdf', 'capa_mindset.jpg', 1, datetime('now'), datetime('now')),
                                        ('Essencialismo', 'Foco no que realmente importa', 'essencialismo.pdf', NULL, 2, datetime('now'), datetime('now'));
                                    ";
            insertCmd.ExecuteNonQuery();

            _initialized = true;
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
