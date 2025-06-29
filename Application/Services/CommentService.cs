using Application.DTOs;
using Application.ServicesInterfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
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

        public CommentService(
            ICommentRepository commentRepository,
            ITaskRepository taskRepository,
            IMapper mapper,
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _taskRepository = taskRepository;
            _mapper = mapper;
            _logger = logger;
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

            var comments = await _commentRepository.GetCommentsForTaskAsync(taskId, cancellationToken);

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

            _mapper.Map(updateCommentDto, comment);
            await _commentRepository.UpdateAsync(comment, cancellationToken);

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

            await _commentRepository.DeleteAsync(comment, cancellationToken);

            _logger.LogInformation(
                "Comment {CommentId} deleted successfully by user {UserId}",
                id, userId);
        }

        public async Task<IEnumerable<CommentDto>> GetAllCommentsAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Admin request to get all comments");

            var comments = await _commentRepository.GetAllCommentsAsync(cancellationToken);

            _logger.LogDebug("Admin retrieved {Count} comments", comments.Count());
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }
    }
}