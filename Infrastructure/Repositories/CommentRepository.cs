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
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comment>> GetCommentsForTaskAsync(string taskId)
        {
            return await _context.Set<Comment>()
                .Where(c => c.TaskaId == taskId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _context.Set<Comment>()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
