using Application.DTOs;
using Application.ServicesInterfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentService> _logger;
        private readonly IMemoryCache _memoryCache;
        private const string AllCommentsCacheKey = "all_comments_cache";
        private const string TaskCommentsCachePrefix = "task_comments_";

        public CommentService(
            ICommentRepository commentRepository,
            ITaskRepository taskRepository,
            IMapper mapper,
            ILogger<CommentService> logger,
            IMemoryCache memoryCache)
        {
            _commentRepository = commentRepository;
            _taskRepository = taskRepository;
            _mapper = mapper;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<CommentDto> CreateCommentAsync(
            CreateCommentDto createCommentDto,
            string authorId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Creating comment for task {TaskId} by user {UserId}",
                createCommentDto.TaskaId, authorId);

            if (!await _taskRepository.ExistsAsync(createCommentDto.TaskaId, cancellationToken))
            {
                _logger.LogWarning("Task {TaskId} not found", createCommentDto.TaskaId);
                throw new KeyNotFoundException("Task not found");
            }

            var comment = _mapper.Map<Comment>(createCommentDto);
            comment.Id = Guid.NewGuid().ToString();
            comment.AuthorId = authorId;
            comment.CreatedAt = DateTime.UtcNow;

            await _commentRepository.AddAsync(comment, cancellationToken);

            _memoryCache.Remove(GetTaskCommentsCacheKey(createCommentDto.TaskaId));
            _memoryCache.Remove(AllCommentsCacheKey);

            _logger.LogInformation(
                "Comment created successfully. CommentId: {CommentId}, TaskId: {TaskId}",
                comment.Id, createCommentDto.TaskaId);

            return _mapper.Map<CommentDto>(comment);
        }

        public async Task<CommentDto> GetCommentByIdAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting comment {CommentId}", id);

            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
            if (comment == null)
            {
                _logger.LogWarning("Comment {CommentId} not found", id);
                throw new KeyNotFoundException("Comment not found");
            }

            _logger.LogDebug("Retrieved comment {CommentId}", id);
            return _mapper.Map<CommentDto>(comment);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsForTaskAsync(
            string taskId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all comments for task {TaskId}", taskId);

            var cacheKey = GetTaskCommentsCacheKey(taskId);

            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Comment> comments))
            {
                comments = await _commentRepository.GetCommentsForTaskAsync(taskId, cancellationToken);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, comments, cacheEntryOptions);

                _logger.LogDebug("Comments for task {TaskId} loaded from repository and cached", taskId);
            }
            else
            {
                _logger.LogDebug("Comments for task {TaskId} loaded from cache", taskId);
            }

            _logger.LogDebug("Retrieved {Count} comments for task {TaskId}", comments.Count(), taskId);
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task UpdateCommentAsync(
            string id,
            UpdateCommentDto updateCommentDto,
            string userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating comment {CommentId} by user {UserId}", id, userId);

            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
            if (comment == null)
            {
                _logger.LogWarning("Comment {CommentId} not found", id);
                throw new KeyNotFoundException("Comment not found");
            }

            if (comment.AuthorId != userId)
            {
                _logger.LogWarning("User {UserId} unauthorized to update comment {CommentId}", userId, id);
                throw new UnauthorizedAccessException("You can only update your own comments");
            }

            var taskId = comment.TaskaId;

            _mapper.Map(updateCommentDto, comment);
            await _commentRepository.UpdateAsync(comment, cancellationToken);

            _memoryCache.Remove(GetTaskCommentsCacheKey(taskId));
            _memoryCache.Remove(AllCommentsCacheKey);

            _logger.LogInformation(
                "Comment {CommentId} updated successfully by user {UserId}",
                id, userId);
        }

        public async Task DeleteCommentAsync(
            string id,
            string userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting comment {CommentId} by user {UserId}", id, userId);

            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
            if (comment == null)
            {
                _logger.LogWarning("Comment {CommentId} not found", id);
                throw new KeyNotFoundException("Comment not found");
            }

            if (comment.AuthorId != userId)
            {
                _logger.LogWarning("User {UserId} unauthorized to delete comment {CommentId}", userId, id);
                throw new UnauthorizedAccessException("You can only delete your own comments");
            }

            var taskId = comment.TaskaId;

            await _commentRepository.DeleteAsync(comment, cancellationToken);

            _memoryCache.Remove(GetTaskCommentsCacheKey(taskId));
            _memoryCache.Remove(AllCommentsCacheKey);

            _logger.LogInformation(
                "Comment {CommentId} deleted successfully by user {UserId}",
                id, userId);
        }

        public async Task<IEnumerable<CommentDto>> GetAllCommentsAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Admin request to get all comments");

            if (!_memoryCache.TryGetValue(AllCommentsCacheKey, out IEnumerable<Comment> comments))
            {
                comments = await _commentRepository.GetAllCommentsAsync(cancellationToken);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(AllCommentsCacheKey, comments, cacheEntryOptions);

                _logger.LogDebug("All comments loaded from repository and cached");
            }
            else
            {
                _logger.LogDebug("All comments loaded from cache");
            }

            _logger.LogDebug("Admin retrieved {Count} comments", comments.Count());
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        private string GetTaskCommentsCacheKey(string taskId)
        {
            return $"{TaskCommentsCachePrefix}{taskId}";
        }
    }
}