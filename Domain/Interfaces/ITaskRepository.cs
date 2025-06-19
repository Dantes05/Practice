using Domain.Entities;
using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public interface ITaskRepository : IBaseRepository<Taska>
    {
        Task<bool> ExistsAsync(string id);
        Task<IEnumerable<Taska>> FindAsync(Expression<Func<Taska, bool>> predicate);
        Task<IEnumerable<Taska>> GetFilteredAndSortedAsync(
            Expression<Func<Taska, bool>> filter,
            string sortBy,
            bool? sortDescending,
            int pageNumber,
            int pageSize);
    }
}