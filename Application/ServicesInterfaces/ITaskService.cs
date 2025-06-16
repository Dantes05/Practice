using Domain.Entities;
using Application.DTOs;

namespace Application.ServicesInterfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, string userId);
        Task<TaskDto> GetTaskByIdAsync(string id);
        Task<IEnumerable<TaskDto>> GetAllTasksAsync(TaskFilterDto filter);
        Task UpdateTaskAsync(string id, UpdateTaskDto updateTaskDto);
        Task DeleteTaskAsync(string id);
        Task ChangeTaskStatusAsync(string id, ChangeTaskStatusDto changeStatusDto);
    }
}