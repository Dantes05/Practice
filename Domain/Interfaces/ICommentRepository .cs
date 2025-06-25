using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICommentRepository : IBaseRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetCommentsForTaskAsync(string taskId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Comment>> GetAllCommentsAsync(CancellationToken cancellationToken = default);
    }
}
