using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Infraestructure.data;

namespace VirtualBuddy.Infraestructure.Persistence
{
    public class Repository<T> : IRepository<T> where T : class
    {

        protected readonly BuddyDBContext _context;
        protected readonly DbSet<T> _db;

        public Repository(BuddyDBContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            var result = await _db.AddAsync(entity);
            return result.Entity;
        }

        public void Delete(T entity)
        {
            _db.Remove(entity);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _db.ToListAsync();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            var query = _db.AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            var query = _db.AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            return await query.Where(predicate).ToListAsync();
        }

        public async Task<T> Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.Update(entity);

            return entity;
        }

    }
}
