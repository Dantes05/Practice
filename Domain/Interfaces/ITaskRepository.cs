using Domain.Entities;
using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public interface ITaskRepository
    {
        Task<Taska> GetByIdAsync(string id);
        Task<IEnumerable<Taska>> GetAllAsync();
        Task<IEnumerable<Taska>> FindAsync(Expression<Func<Taska, bool>> predicate);
        Task<IEnumerable<Taska>> GetFilteredAndSortedAsync(
            Expression<Func<Taska, bool>> filter,
            string sortBy,
            bool? sortDescending,
            int pageNumber,
            int pageSize);
        Task AddAsync(Taska task);
        Task UpdateAsync(Taska task);
        Task DeleteAsync(Taska task);
        Task<bool> ExistsAsync(string id);
    }
}