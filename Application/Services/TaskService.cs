using Domain.Entities;
using Domain.Interfaces;
using Application.DTOs;
using Application.ServicesInterfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public TaskService(ITaskRepository taskRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, string userId)
        {
            var task = _mapper.Map<Taska>(createTaskDto);
            task.Id = Guid.NewGuid().ToString();
            task.CreatorId = userId;
            task.Status = "New";
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.AddAsync(task);
            return _mapper.Map<TaskDto>(task);
        }

        public async Task<TaskDto> GetTaskByIdAsync(string id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            return _mapper.Map<TaskDto>(task);
        }

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(TaskFilterDto filter)
        {
            var tasks = await _taskRepository.FindAsync(t =>
                (string.IsNullOrEmpty(filter.Status) || t.Status == filter.Status) &&
                (string.IsNullOrEmpty(filter.Priority) || t.Priority == filter.Priority) &&
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value) &&
                (string.IsNullOrEmpty(filter.AssigneeId) || t.AssigneeId == filter.AssigneeId) &&
                (string.IsNullOrEmpty(filter.CreatorId) || t.CreatorId == filter.CreatorId));

            return _mapper.Map<IEnumerable<TaskDto>>(tasks)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);
        }

        public async Task UpdateTaskAsync(string id, UpdateTaskDto updateTaskDto)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            _mapper.Map(updateTaskDto, task);
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);
        }

        public async Task DeleteTaskAsync(string id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            await _taskRepository.DeleteAsync(task);
        }

        public async Task ChangeTaskStatusAsync(string id, ChangeTaskStatusDto changeStatusDto)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            task.Status = changeStatusDto.Status;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);
        }
    }
}