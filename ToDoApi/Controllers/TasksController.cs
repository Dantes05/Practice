using Microsoft.AspNetCore.Mvc;
using Application.ServicesInterfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            ITaskService taskService,
            ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        private string GetUserIdFromToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token");
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation(
                "Creating task. Title: {Title}, User: {UserId}",
                createTaskDto.Title, userId);

            try
            {
                var task = await _taskService.CreateTaskAsync(createTaskDto, userId);

                _logger.LogInformation(
                    "Task created successfully. TaskId: {TaskId}, Title: {Title}",
                    task.Id, task.Title);

                return CreatedAtAction(
                    nameof(GetTask),
                    new { id = task.Id },
                    task);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Unauthorized attempt to create task by user {UserId}",
                    userId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating task. Title: {Title}, User: {UserId}",
                    createTaskDto.Title, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(string id)
        {
            _logger.LogDebug("Getting task {TaskId}", id);

            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);

                _logger.LogDebug(
                    "Retrieved task {TaskId} with status {Status}",
                    id, task.Status);

                return Ok(task);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Task not found: {TaskId}", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting task {TaskId}",
                    id);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks([FromQuery] TaskFilterDto filter)
        {
            _logger.LogDebug(
                "Getting tasks with filter. Status: {Status}, Priority: {Priority}",
                filter.Status, filter.Priority);

            try
            {
                var tasks = await _taskService.GetAllTasksAsync(filter);

                _logger.LogDebug(
                    "Retrieved {Count} tasks with filter",
                    tasks.Count());

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting tasks with filter");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(
            string id,
            [FromBody] UpdateTaskDto updateTaskDto)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation(
                "Updating task {TaskId} by user {UserId}",
                id, userId);

            try
            {
                await _taskService.UpdateTaskAsync(id, updateTaskDto, userId);

                _logger.LogInformation(
                    "Task {TaskId} updated successfully by user {UserId}",
                    id, userId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Unauthorized attempt to update task {TaskId} by user {UserId}",
                    id, userId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning(
                    "Task not found during update. TaskId: {TaskId}, User: {UserId}",
                    id, userId);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating task {TaskId} by user {UserId}",
                    id, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(string id)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation(
                "Deleting task {TaskId} by user {UserId}",
                id, userId);

            try
            {
                await _taskService.DeleteTaskAsync(id, userId);

                _logger.LogInformation(
                    "Task {TaskId} deleted successfully by user {UserId}",
                    id, userId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Unauthorized attempt to delete task {TaskId} by user {UserId}",
                    id, userId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning(
                    "Task not found during deletion. TaskId: {TaskId}, User: {UserId}",
                    id, userId);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting task {TaskId} by user {UserId}",
                    id, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(
            string id,
            [FromBody] ChangeTaskStatusDto changeStatusDto)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation(
                "Changing status for task {TaskId} to {Status} by user {UserId}",
                id, changeStatusDto.Status, userId);

            try
            {
                await _taskService.ChangeTaskStatusAsync(id, changeStatusDto, userId);

                _logger.LogInformation(
                    "Status changed successfully for task {TaskId} to {Status} by user {UserId}",
                    id, changeStatusDto.Status, userId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Unauthorized status change attempt for task {TaskId} by user {UserId}",
                    id, userId);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning(
                    "Task not found during status change. TaskId: {TaskId}, User: {UserId}",
                    id, userId);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error changing status for task {TaskId} by user {UserId}",
                    id, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportTasks([FromQuery] TaskFilterDto filter)
        {
            _logger.LogInformation(
                "Exporting tasks with filter. Status: {Status}, Priority: {Priority}",
                filter.Status, filter.Priority);

            try
            {
                var csvBytes = await _taskService.ExportTasksToCsvAsync(filter);

                _logger.LogInformation(
                    "Successfully exported tasks with filter");

                return File(
                    csvBytes,
                    "text/csv",
                    $"tasks_export_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error exporting tasks with filter");
                return BadRequest(ex.Message);
            }
        }
    }
}