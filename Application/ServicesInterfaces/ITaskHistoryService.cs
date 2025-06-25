using Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.ServicesInterfaces
{
    public interface ITaskHistoryService
    {
        Task<TaskHistoryDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskHistoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskHistoryDto>> GetByTaskIdAsync(string taskId, CancellationToken cancellationToken = default);
    }
}