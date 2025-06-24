using Application.DTOs;
using Application.ServicesInterfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ToDoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(
            ICommentService commentService,
            ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment(
            string taskId,
            [FromBody] CreateCommentDto createCommentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized attempt to create comment for task {TaskId}", taskId);
                return Unauthorized();
            }

            _logger.LogInformation(
                "Creating comment for task {TaskId} by user {UserId}",
                taskId, userId);

            try
            {
                createCommentDto.TaskaId = taskId;
                var comment = await _commentService.CreateCommentAsync(createCommentDto, userId);

                _logger.LogInformation(
                    "Comment created successfully. CommentId: {CommentId}, TaskId: {TaskId}",
                    comment.Id, taskId);

                return CreatedAtAction(
                    nameof(GetComment),
                    new { taskId, id = comment.Id },
                    comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating comment for task {TaskId} by user {UserId}",
                    taskId, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment(string taskId, string id)
        {
            _logger.LogDebug(
                "Getting comment {CommentId} for task {TaskId}",
                id, taskId);

            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);

                _logger.LogDebug(
                    "Retrieved comment {CommentId} for task {TaskId}",
                    id, taskId);

                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting comment {CommentId} for task {TaskId}",
                    id, taskId);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForTask(string taskId)
        {
            _logger.LogDebug(
                "Getting all comments for task {TaskId}",
                taskId);

            try
            {
                var comments = await _commentService.GetCommentsForTaskAsync(taskId);

                _logger.LogDebug(
                    "Retrieved {Count} comments for task {TaskId}",
                    comments.Count(), taskId);

                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting comments for task {TaskId}",
                    taskId);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(
            string taskId,
            string id,
            [FromBody] UpdateCommentDto updateCommentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning(
                    "Unauthorized attempt to update comment {CommentId}",
                    id);
                return Unauthorized();
            }

            _logger.LogInformation(
                "Updating comment {CommentId} by user {UserId}",
                id, userId);

            try
            {
                await _commentService.UpdateCommentAsync(id, updateCommentDto, userId);

                _logger.LogInformation(
                    "Comment {CommentId} updated successfully by user {UserId}",
                    id, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating comment {CommentId} by user {UserId}",
                    id, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(string taskId, string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning(
                    "Unauthorized attempt to delete comment {CommentId}",
                    id);
                return Unauthorized();
            }

            _logger.LogInformation(
                "Deleting comment {CommentId} by user {UserId}",
                id, userId);

            try
            {
                await _commentService.DeleteCommentAsync(id, userId);

                _logger.LogInformation(
                    "Comment {CommentId} deleted successfully by user {UserId}",
                    id, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting comment {CommentId} by user {UserId}",
                    id, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        [Authorize(Policy = "OnlyAdminUsers")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllComments()
        {
            _logger.LogDebug("Admin request to get all comments");

            try
            {
                var comments = await _commentService.GetAllCommentsAsync();

                _logger.LogDebug(
                    "Admin retrieved {Count} comments",
                    comments.Count());

                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Admin error getting all comments");
                return BadRequest(ex.Message);
            }
        }
    }
}