using ASC.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASC.DataAccess
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task<T?> FindAsync(string id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> FindAllAsync() => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAllByAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task<T> CreateAsync(T entity) { await _dbSet.AddAsync(entity); return entity; }

        public Task<T> UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<int> CountAllAsync() => await _dbSet.CountAsync();
    }
}
