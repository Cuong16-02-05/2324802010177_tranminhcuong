using System.Linq.Expressions;

namespace ASC.DataAccess.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> FindAsync(string id);
        Task<IEnumerable<T>> FindAllAsync();
        Task<IEnumerable<T>> FindAllByAsync(Expression<Func<T, bool>> predicate);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAllAsync();
    }
}
