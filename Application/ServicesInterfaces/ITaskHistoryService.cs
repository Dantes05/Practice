using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesInterfaces
{
    public interface ITaskHistoryService
    {
        Task<TaskHistoryDto> GetByIdAsync(string id);
        Task<IEnumerable<TaskHistoryDto>> GetAllAsync();
        Task<IEnumerable<TaskHistoryDto>> GetByTaskIdAsync(string taskId);
    }
}
