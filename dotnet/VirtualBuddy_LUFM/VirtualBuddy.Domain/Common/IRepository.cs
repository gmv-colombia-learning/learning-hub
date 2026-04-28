using System.Linq.Expressions;

namespace VirtualBuddy.Domain.Common
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null);

        Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null);

        Task<List<T>> GetAllAsync();

        Task<T> AddAsync(T entity);

        Task<T> Update(T entity);

        void Delete(T entity);
    }
}
