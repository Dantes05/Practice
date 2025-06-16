using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Taska> GetByIdAsync(string id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<IEnumerable<Taska>> GetAllAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<IEnumerable<Taska>> FindAsync(Expression<Func<Taska, bool>> predicate)
        {
            return await _context.Tasks.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(Taska task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Taska task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Taska task)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Taska>> GetFilteredAndSortedAsync(Expression<Func<Taska, bool>> filter, string sortBy, bool? sortDescending, int pageNumber, int pageSize)
        {
            var query = _context.Tasks.AsQueryable().Where(filter);

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
        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Tasks.AnyAsync(t => t.Id == id);
        }
    }
}