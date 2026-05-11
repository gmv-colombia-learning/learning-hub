using Microsoft.EntityFrameworkCore;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Infraestructure.data;

namespace VirtualBuddy.Infraestructure.Persistence
{
    public class Repository : IRepository
    {

        protected readonly BuddyDBContext _dbContext;

        public Repository(BuddyDBContext context)
        {
            _dbContext = context;
        }

        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbContext.AddAsync(entity);

            return entity;
        }

        public async Task<ICollection<T>> AddRangeAsync<T>(ICollection<T> entities) where T : class
        {
            await _dbContext.AddRangeAsync(entities);
            return entities;
        }

        public T Update<T>(T entity) where T : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbContext.Update(entity);

            return entity;
        }

        public void Delete<T>(T entity) where T : class
        {
            _dbContext.Remove(entity);
        }

        public async Task<ICollection<T>> GetAllAsync<T>() where T : class
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<ICollection<T>> GetAllWithSpecAsync<T>(ISpecification<T> spec) where T : class
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<T?> GetByIdAsync<T>(params object[] keyValues) where T : class
        {
            return await _dbContext.Set<T>().FindAsync(keyValues);
        }

        public async Task<T?> GetEntityWithSpecAsync<T>(ISpecification<T> spec) where T : class
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }

        private IQueryable<T> ApplySpecification<T>(ISpecification<T> spec) where T : class
        {
            return SpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>().AsQueryable(), spec);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
