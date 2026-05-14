using ASC.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASC.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
        Task<bool> SaveEntitiesAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private readonly Dictionary<string, object> _repositories = new();

        public UnitOfWork(DbContext dbContext) { _dbContext = dbContext; }

        public IRepository<T> Repository<T>() where T : class
        {
            var key = typeof(T).Name;
            if (!_repositories.ContainsKey(key))
                _repositories[key] = new Repository<T>(_dbContext);
            return (IRepository<T>)_repositories[key];
        }

        public async Task<int> SaveChangesAsync() => await _dbContext.SaveChangesAsync();
        public async Task<bool> SaveEntitiesAsync() => await _dbContext.SaveChangesAsync() > 0;
        public void Dispose() => _dbContext.Dispose();
    }
}
