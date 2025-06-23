using Domain.Entities;
using Domain.Interfaces;
using Application.DTOs;
using Application.ServicesInterfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Domain.Enums;
using System.Reflection;

namespace Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IMapper _mapper;

        public TaskService(
            ITaskRepository taskRepository,
            ITaskHistoryRepository taskHistoryRepository,
            IMapper mapper)
        {
            _taskRepository = taskRepository;
            _taskHistoryRepository = taskHistoryRepository;
            _mapper = mapper;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, string userId)
        {
            var task = _mapper.Map<Taska>(createTaskDto);
            task.Id = Guid.NewGuid().ToString();
            task.CreatorId = userId;
            task.Status = TaskaStatus.New;
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.AddAsync(task);

            await AddHistoryRecord(
                task.Id,
                userId,
                "Task",
                null,
                "Created",
                $"Task created by user {userId}");

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
            Expression<Func<Taska, bool>> predicate = t =>
                (!filter.Status.HasValue || t.Status == filter.Status.Value) &&
                (!filter.Priority.HasValue || t.Priority == filter.Priority.Value) &&
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value) &&
                (!filter.DueDateFrom.HasValue || t.DueDate >= filter.DueDateFrom.Value) &&
                (!filter.DueDateTo.HasValue || t.DueDate <= filter.DueDateTo.Value) &&
                (string.IsNullOrEmpty(filter.AssigneeId) || t.AssigneeId == filter.AssigneeId) &&
                (string.IsNullOrEmpty(filter.CreatorId) || t.CreatorId == filter.CreatorId);

            var tasks = await _taskRepository.GetFilteredAndSortedAsync(
                predicate,
                filter.SortBy,
                filter.SortDescending,
                filter.PageNumber,
                filter.PageSize);

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task UpdateTaskAsync(string id, UpdateTaskDto updateTaskDto, string userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            var oldValues = new Dictionary<string, string>
            {
                { "Title", task.Title },
                { "Description", task.Description },
                { "Priority", task.Priority.ToString() },
                { "DueDate", task.DueDate.ToString("o") },
                { "AssigneeId", task.AssigneeId }
            };

            _mapper.Map(updateTaskDto, task);
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);

            foreach (var change in oldValues)
            {
                var newValue = GetPropertyValue(task, change.Key);
                if (change.Value != newValue)
                {
                    await AddHistoryRecord(
                        task.Id,
                        userId,
                        change.Key,
                        change.Value,
                        newValue);
                }
            }
        }

        public async Task DeleteTaskAsync(string id, string userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            await AddHistoryRecord(
                task.Id,
                userId,
                "Task",
                "Exists",
                "Deleted",
                $"Task was deleted by user {userId}");

            await _taskRepository.DeleteAsync(task);
        }

        public async Task ChangeTaskStatusAsync(string id, ChangeTaskStatusDto changeStatusDto, string userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            var oldStatus = task.Status.ToString();
            task.Status = changeStatusDto.Status;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);

            await AddHistoryRecord(
                task.Id,
                userId,
                "Status",
                oldStatus,
                task.Status.ToString(),
                $"Status changed from {oldStatus} to {task.Status} by user {userId}");
        }

        private async Task AddHistoryRecord(
            string taskId,
            string userId,
            string field,
            string oldValue,
            string newValue)
        {
            var history = new TaskHistory
            {
                TaskaId = taskId,
                ChangedById = userId,
                ChangedField = field,
                OldValue = oldValue ?? string.Empty,
                NewValue = newValue ?? string.Empty,
                ChangedAt = DateTime.UtcNow
            };

            await _taskHistoryRepository.AddAsync(history);
        }

        private async Task AddHistoryRecord(
            string taskId,
            string userId,
            string field,
            string oldValue,
            string newValue,
            string additionalInfo)
        {
            var history = new TaskHistory
            {
                TaskaId = taskId,
                ChangedById = userId,
                ChangedField = field,
                OldValue = oldValue ?? string.Empty,
                NewValue = newValue ?? string.Empty,
                ChangedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                history.NewValue = $"{newValue} | {additionalInfo}";
            }

            await _taskHistoryRepository.AddAsync(history);
        }

        private string GetPropertyValue(Taska task, string propertyName)
        {
            var property = typeof(Taska).GetProperty(propertyName);
            if (property == null) return string.Empty;

            var value = property.GetValue(task);
            if (value == null) return string.Empty;

            if (value is DateTime dateTimeValue)
            {
                return dateTimeValue.ToString("o");
            }

            return value.ToString();
        }

        public async Task<byte[]> ExportTasksToCsvAsync(TaskFilterDto filter)
        {
           
            Expression<Func<Taska, bool>> predicate = t =>
                (!filter.Status.HasValue || t.Status == filter.Status.Value) &&
                (!filter.Priority.HasValue || t.Priority == filter.Priority.Value) &&
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value) &&
                (!filter.DueDateFrom.HasValue || t.DueDate >= filter.DueDateFrom.Value) &&
                (!filter.DueDateTo.HasValue || t.DueDate <= filter.DueDateTo.Value) &&
                (string.IsNullOrEmpty(filter.AssigneeId) || t.AssigneeId == filter.AssigneeId) &&
                (string.IsNullOrEmpty(filter.CreatorId) || t.CreatorId == filter.CreatorId);

            var tasks = await _taskRepository.FindAsync(predicate);

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvHelper.CsvWriter(streamWriter, System.Globalization.CultureInfo.InvariantCulture);

            csvWriter.WriteHeader<TaskCsvDto>();
            await csvWriter.NextRecordAsync();

            var csvRecords = _mapper.Map<IEnumerable<TaskCsvDto>>(tasks);
            await csvWriter.WriteRecordsAsync(csvRecords);

            await streamWriter.FlushAsync();
            return memoryStream.ToArray();
        }
    }
}