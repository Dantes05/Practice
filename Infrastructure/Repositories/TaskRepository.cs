using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class TaskRepository : BaseRepository<Taska>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Set<Taska>().AnyAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Taska>> FindAsync(Expression<Func<Taska, bool>> predicate)
        {
            return await _context.Set<Taska>().Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<Taska>> GetFilteredAndSortedAsync(
            Expression<Func<Taska, bool>> filter,
            string sortBy,
            bool? sortDescending,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Set<Taska>().AsQueryable().Where(filter);

            query = sortBy?.ToLower() switch
            {
                "createdat" => sortDescending == true
                    ? query.OrderByDescending(t => t.CreatedAt)
                    : query.OrderBy(t => t.CreatedAt),
                "duedate" => sortDescending == true
                    ? query.OrderByDescending(t => t.DueDate)
                    : query.OrderBy(t => t.DueDate),
                "priority" => sortDescending == true
                    ? query.OrderByDescending(t => t.Priority)
                    : query.OrderBy(t => t.Priority),
                _ => query
            };

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}