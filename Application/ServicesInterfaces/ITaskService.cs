using Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.ServicesInterfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, string userId, CancellationToken cancellationToken = default);
        Task<TaskDto> GetTaskByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskDto>> GetAllTasksAsync(TaskFilterDto filter, CancellationToken cancellationToken = default);
        Task UpdateTaskAsync(string id, UpdateTaskDto updateTaskDto, string userId, CancellationToken cancellationToken = default);
        Task DeleteTaskAsync(string id, string userId, CancellationToken cancellationToken = default);
        Task ChangeTaskStatusAsync(string id, ChangeTaskStatusDto changeStatusDto, string userId, CancellationToken cancellationToken = default);
        Task<byte[]> ExportTasksToCsvAsync(TaskFilterDto filter, CancellationToken cancellationToken = default);
    }
}