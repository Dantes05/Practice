using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TaskHistoryRepository : BaseRepository<TaskHistory>, ITaskHistoryRepository
    {
        public TaskHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskHistory>> GetByTaskIdAsync(string taskId, CancellationToken cancellationToken = default)
        {
            return await _context.TaskHistories
                .Where(th => th.TaskaId == taskId)
                .OrderByDescending(th => th.ChangedAt)
                .ToListAsync(cancellationToken);
        }
    }
}