using Npgsql;

namespace CleanBrilliant.DataSource
{
    public class UnitOfWork : IDisposable
    {
        private readonly NpgsqlConnection _connection;
        private NpgsqlTransaction? _transaction;
        private readonly List<Func<NpgsqlTransaction, Task>> _newOperations = new();
        private readonly List<Func<NpgsqlTransaction, Task>> _dirtyOperations = new();
        private readonly List<Func<NpgsqlTransaction, Task>> _deletedOperations = new();

        public UnitOfWork(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
        }

        public void RegisterNew(Func<NpgsqlTransaction, Task> operation) => _newOperations.Add(operation);
        public void RegisterDirty(Func<NpgsqlTransaction, Task> operation) => _dirtyOperations.Add(operation);
        public void RegisterDeleted(Func<NpgsqlTransaction, Task> operation) => _deletedOperations.Add(operation);

        public async Task Commit()
        {
            await _connection.OpenAsync();
            _transaction = await _connection.BeginTransactionAsync();
            try
            {
                foreach (var op in _newOperations) await op(_transaction);
                foreach (var op in _dirtyOperations) await op(_transaction);
                foreach (var op in _deletedOperations) await op(_transaction);
                await _transaction.CommitAsync();
            }
            catch
            {
                await Rollback();
                throw;
            }
            finally
            {
                _newOperations.Clear();
                _dirtyOperations.Clear();
                _deletedOperations.Clear();
            }
        }

        public async Task Rollback()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection.Dispose();
        }
    }
}
