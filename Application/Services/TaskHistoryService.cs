using Application.DTOs;
using Application.ServicesInterfaces;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TaskHistoryService : ITaskHistoryService
    {
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskHistoryService> _logger;
        private readonly IMemoryCache _memoryCache;
        private const string AllHistoryCacheKey = "all_history_records";
        private const string TaskHistoryCachePrefix = "task_history_";

        public TaskHistoryService(
            ITaskHistoryRepository taskHistoryRepository,
            IMapper mapper,
            ILogger<TaskHistoryService> logger,
            IMemoryCache memoryCache)
        {
            _taskHistoryRepository = taskHistoryRepository;
            _mapper = mapper;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<TaskHistoryDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting history record {RecordId}", id);

            var history = await _taskHistoryRepository.GetByIdAsync(id, cancellationToken);
            if (history == null)
            {
                _logger.LogWarning("History record {RecordId} not found", id);
                throw new KeyNotFoundException("History record not found");
            }

            _logger.LogDebug("Successfully retrieved history record {RecordId}", id);
            return _mapper.Map<TaskHistoryDto>(history);
        }

        public async Task<IEnumerable<TaskHistoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all history records");

            if (_memoryCache.TryGetValue(AllHistoryCacheKey, out IEnumerable<TaskHistoryDto> cachedResult))
            {
                _logger.LogDebug("Returning cached history records");
                return cachedResult;
            }

            var histories = await _taskHistoryRepository.GetAllAsync(cancellationToken);
            var result = _mapper.Map<IEnumerable<TaskHistoryDto>>(histories);

            _memoryCache.Set(AllHistoryCacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            _logger.LogDebug("Retrieved {Count} history records", result.Count());
            return result;
        }

        public async Task<IEnumerable<TaskHistoryDto>> GetByTaskIdAsync(
            string taskId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting history records for task {TaskId}", taskId);

            var cacheKey = $"{TaskHistoryCachePrefix}{taskId}";
            if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<TaskHistoryDto> cachedResult))
            {
                _logger.LogDebug("Returning cached history records for task {TaskId}", taskId);
                return cachedResult;
            }

            var histories = await _taskHistoryRepository.GetByTaskIdAsync(taskId, cancellationToken);
            var result = _mapper.Map<IEnumerable<TaskHistoryDto>>(histories);

            _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            _logger.LogDebug("Retrieved {Count} history records for task {TaskId}", result.Count(), taskId);
            return result;
        }

        public void InvalidateTaskHistoryCache(string taskId)
        {
            var cacheKey = $"{TaskHistoryCachePrefix}{taskId}";
            _memoryCache.Remove(cacheKey);
            _memoryCache.Remove(AllHistoryCacheKey);
            _logger.LogDebug("Invalidated history cache for task {TaskId}", taskId);
        }
    }
}