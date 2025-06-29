using Microsoft.AspNetCore.Mvc;
using Application.ServicesInterfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Infrastructure.Filters;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidationFilter))]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private string GetUserIdFromToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask(
            [FromBody] CreateTaskDto createTaskDto,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserIdFromToken();
            var task = await _taskService.CreateTaskAsync(createTaskDto, userId, cancellationToken);
            return CreatedAtAction(
                nameof(GetTask),
                new { id = task.Id },
                task);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(
            string id,
            CancellationToken cancellationToken = default)
        {
            var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);
            return Ok(task);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(
            [FromQuery] TaskFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var tasks = await _taskService.GetAllTasksAsync(filter, cancellationToken);
            return Ok(tasks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(
            string id,
            [FromBody] UpdateTaskDto updateTaskDto,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserIdFromToken();
            await _taskService.UpdateTaskAsync(id, updateTaskDto, userId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(
            string id,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserIdFromToken();
            await _taskService.DeleteTaskAsync(id, userId, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(
            string id,
            [FromBody] ChangeTaskStatusDto changeStatusDto,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserIdFromToken();
            await _taskService.ChangeTaskStatusAsync(id, changeStatusDto, userId, cancellationToken);
            return NoContent();
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportTasks(
            [FromQuery] TaskFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var csvBytes = await _taskService.ExportTasksToCsvAsync(filter, cancellationToken);
            return File(
                csvBytes,
                "text/csv",
                $"tasks_export_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
    }
}