using Application.DTOs;
using Application.ServicesInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ToDoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment(
            string taskId,
            [FromBody] CreateCommentDto createCommentDto,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            createCommentDto.TaskaId = taskId;
            var comment = await _commentService.CreateCommentAsync(createCommentDto, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetComment),
                new { taskId, id = comment.Id },
                comment);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment(
            string taskId,
            string id,
            CancellationToken cancellationToken = default)
        {
            var comment = await _commentService.GetCommentByIdAsync(id, cancellationToken);
            return Ok(comment);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForTask(
            string taskId,
            CancellationToken cancellationToken = default)
        {
            var comments = await _commentService.GetCommentsForTaskAsync(taskId, cancellationToken);
            return Ok(comments);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(
            string taskId,
            string id,
            [FromBody] UpdateCommentDto updateCommentDto,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _commentService.UpdateCommentAsync(id, updateCommentDto, userId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(
            string taskId,
            string id,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _commentService.DeleteCommentAsync(id, userId, cancellationToken);
            return NoContent();
        }

        [HttpGet("all")]
        [Authorize(Policy = "OnlyAdminUsers")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllComments(
            CancellationToken cancellationToken = default)
        {
            var comments = await _commentService.GetAllCommentsAsync(cancellationToken);
            return Ok(comments);
        }
    }
}