using Application.DTOs;
using Application.ServicesInterfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<ActionResult<CommentDto>> CreateComment(string taskId, [FromBody] CreateCommentDto createCommentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                createCommentDto.TaskaId = taskId;
                var comment = await _commentService.CreateCommentAsync(createCommentDto, userId);
                return CreatedAtAction(nameof(GetComment), new { taskId, id = comment.Id }, comment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment(string taskId, string id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForTask(string taskId)
        {
            try
            {
                var comments = await _commentService.GetCommentsForTaskAsync(taskId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(string taskId, string id, [FromBody] UpdateCommentDto updateCommentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _commentService.UpdateCommentAsync(id, updateCommentDto, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(string taskId, string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _commentService.DeleteCommentAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        [Authorize(Policy = "OnlyAdminUsers")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllComments()
        {
            try
            {
                var comments = await _commentService.GetAllCommentsAsync();
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
