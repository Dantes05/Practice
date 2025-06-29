using Application.DTOs;
using Application.ServicesInterfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskService> _logger;
        private readonly IMemoryCache _memoryCache;
        private const string AllTasksCacheKey = "all_tasks_filtered";
        private readonly ITaskHistoryService _taskHistoryService;

        public TaskService(
            ITaskRepository taskRepository,
            ITaskHistoryRepository taskHistoryRepository,
            IMapper mapper,
            ILogger<TaskService> logger,
            IMemoryCache memoryCache,
            ITaskHistoryService taskHistoryService)
        {
            _taskRepository = taskRepository;
            _taskHistoryRepository = taskHistoryRepository;
            _mapper = mapper;
            _logger = logger;
            _memoryCache = memoryCache;
            _taskHistoryService = taskHistoryService;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Creating task. Title: {Title}, User: {UserId}",
                createTaskDto.Title, userId);

            try
            {
                var task = _mapper.Map<Taska>(createTaskDto);
                task.Id = Guid.NewGuid().ToString();
                task.CreatorId = userId;
                task.Status = TaskaStatus.New;
                task.CreatedAt = DateTime.UtcNow;
                task.UpdatedAt = DateTime.UtcNow;

                await _taskRepository.AddAsync(task, cancellationToken);

                await AddHistoryRecord(
                    task.Id,
                    userId,
                    "Task",
                    null,
                    "Created",
                    $"Task created by user {userId}",
                    cancellationToken);

                InvalidateTaskCaches();

                _logger.LogInformation(
                    "Task created successfully. TaskId: {TaskId}, Title: {Title}",
                    task.Id, task.Title);

                return _mapper.Map<TaskDto>(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating task. Title: {Title}, User: {UserId}",
                    createTaskDto.Title, userId);
                throw;
            }
        }

        public async Task<TaskDto> GetTaskByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting task {TaskId}", id);

            try
            {
                var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
                if (task == null)
                {
                    _logger.LogWarning("Task not found: {TaskId}", id);
                    throw new KeyNotFoundException("Task not found");
                }

                _logger.LogDebug(
                    "Retrieved task {TaskId} with status {Status}",
                    id, task.Status);

                return _mapper.Map<TaskDto>(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting task {TaskId}",
                    id);
                throw;
            }
        }

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(TaskFilterDto filter, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug(
                "Getting tasks with filter. Status: {Status}, Priority: {Priority}",
                filter.Status, filter.Priority);

            try
            {
                var cacheKey = $"{AllTasksCacheKey}_{filter.GetHashCode()}";
                if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<TaskDto> cachedResult))
                {
                    _logger.LogDebug("Returning cached tasks with filter");
                    return cachedResult;
                }

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
                    filter.PageSize,
                    cancellationToken);

                var result = _mapper.Map<IEnumerable<TaskDto>>(tasks);

                _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                _logger.LogDebug(
                    "Retrieved {Count} tasks with filter",
                    result.Count());

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting tasks with filter");
                throw;
            }
        }

        public async Task UpdateTaskAsync(string id, UpdateTaskDto updateTaskDto, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Updating task {TaskId} by user {UserId}",
                id, userId);

            try
            {
                var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
                if (task == null)
                {
                    _logger.LogWarning(
                        "Task not found during update. TaskId: {TaskId}, User: {UserId}",
                        id, userId);
                    throw new KeyNotFoundException("Task not found");
                }

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

                await _taskRepository.UpdateAsync(task, cancellationToken);

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
                            newValue,
                            cancellationToken);
                    }
                }

                InvalidateTaskCaches();
                _taskHistoryService.InvalidateTaskHistoryCache(id);

                _logger.LogInformation(
                    "Task {TaskId} updated successfully by user {UserId}",
                    id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating task {TaskId} by user {UserId}",
                    id, userId);
                throw;
            }
        }

        public async Task DeleteTaskAsync(string id, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Deleting task {TaskId} by user {UserId}",
                id, userId);

            try
            {
                var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
                if (task == null)
                {
                    _logger.LogWarning(
                        "Task not found during deletion. TaskId: {TaskId}, User: {UserId}",
                        id, userId);
                    throw new KeyNotFoundException("Task not found");
                }

                await AddHistoryRecord(
                    task.Id,
                    userId,
                    "Task",
                    "Exists",
                    "Deleted",
                    $"Task was deleted by user {userId}",
                    cancellationToken);

                await _taskRepository.DeleteAsync(task, cancellationToken);

                InvalidateTaskCaches();
                _taskHistoryService.InvalidateTaskHistoryCache(id);

                _logger.LogInformation(
                    "Task {TaskId} deleted successfully by user {UserId}",
                    id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting task {TaskId} by user {UserId}",
                    id, userId);
                throw;
            }
        }

        public async Task ChangeTaskStatusAsync(string id, ChangeTaskStatusDto changeStatusDto, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Changing status for task {TaskId} to {Status} by user {UserId}",
                id, changeStatusDto.Status, userId);

            try
            {
                var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
                if (task == null)
                {
                    _logger.LogWarning(
                        "Task not found during status change. TaskId: {TaskId}, User: {UserId}",
                        id, userId);
                    throw new KeyNotFoundException("Task not found");
                }

                var oldStatus = task.Status.ToString();
                task.Status = changeStatusDto.Status;
                task.UpdatedAt = DateTime.UtcNow;

                await _taskRepository.UpdateAsync(task, cancellationToken);

                await AddHistoryRecord(
                    task.Id,
                    userId,
                    "Status",
                    oldStatus,
                    task.Status.ToString(),
                    $"Status changed from {oldStatus} to {task.Status} by user {userId}",
                    cancellationToken);

                InvalidateTaskCaches();
                _taskHistoryService.InvalidateTaskHistoryCache(id);

                _logger.LogInformation(
                    "Status changed successfully for task {TaskId} to {Status} by user {UserId}",
                    id, changeStatusDto.Status, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error changing status for task {TaskId} by user {UserId}",
                    id, userId);
                throw;
            }
        }

        public async Task<byte[]> ExportTasksToCsvAsync(TaskFilterDto filter, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Exporting tasks with filter. Status: {Status}, Priority: {Priority}",
                filter.Status, filter.Priority);

            try
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

                var tasks = await _taskRepository.FindAsync(predicate, cancellationToken);

                using var memoryStream = new MemoryStream();
                using var streamWriter = new StreamWriter(memoryStream);
                using var csvWriter = new CsvHelper.CsvWriter(streamWriter, System.Globalization.CultureInfo.InvariantCulture);

                csvWriter.WriteHeader<TaskCsvDto>();
                await csvWriter.NextRecordAsync();

                var csvRecords = _mapper.Map<IEnumerable<TaskCsvDto>>(tasks);
                await csvWriter.WriteRecordsAsync(csvRecords, cancellationToken);

                await streamWriter.FlushAsync();

                _logger.LogInformation(
                    "Successfully exported tasks with filter");

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error exporting tasks with filter");
                throw;
            }
        }

        private async Task AddHistoryRecord(
            string taskId,
            string userId,
            string field,
            string oldValue,
            string newValue,
            CancellationToken cancellationToken = default)
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

            await _taskHistoryRepository.AddAsync(history, cancellationToken);
        }

        private async Task AddHistoryRecord(
            string taskId,
            string userId,
            string field,
            string oldValue,
            string newValue,
            string additionalInfo,
            CancellationToken cancellationToken = default)
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

            await _taskHistoryRepository.AddAsync(history, cancellationToken);
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

        private void InvalidateTaskCaches()
        {
            
             _memoryCache.Remove(AllTasksCacheKey);
                
             _logger.LogDebug("Invalidated all task caches");
        }
    }
}