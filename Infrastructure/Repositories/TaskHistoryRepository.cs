using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TaskHistoryRepository : BaseRepository<TaskHistory>, ITaskHistoryRepository
    {
        public TaskHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskHistory>> GetByTaskIdAsync(string taskId)
        {
            return await _context.TaskHistories
                .Where(th => th.TaskaId == taskId)
                .OrderByDescending(th => th.ChangedAt)
                .ToListAsync();
        }
    }
}
