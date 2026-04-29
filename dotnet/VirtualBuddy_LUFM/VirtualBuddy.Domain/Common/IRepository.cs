namespace VirtualBuddy.Domain.Common
{
    public interface IRepository
    {
        Task<T> AddAsync<T>(T entity) where T : class;
        Task<ICollection<T>> AddRangeAsync<T>(ICollection<T> entities) where T : class;

        T Update<T>(T entity) where T : class;

        void Delete<T>(T entity) where T : class;
        Task<ICollection<T>> GetAllAsync<T>() where T : class;
    }
}
