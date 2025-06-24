using Application.DTOs;
using Application.ServicesInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ToDoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/history")]
    public class TaskHistoryController : ControllerBase
    {
        private readonly ITaskHistoryService _taskHistoryService;
        private readonly ILogger<TaskHistoryController> _logger;

        public TaskHistoryController(
            ITaskHistoryService taskHistoryService,
            ILogger<TaskHistoryController> logger)
        {
            _taskHistoryService = taskHistoryService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskHistoryDto>> GetHistoryRecord(string id)
        {
            _logger.LogDebug("Getting history record {RecordId}", id);

            try
            {
                var record = await _taskHistoryService.GetByIdAsync(id);

                _logger.LogDebug(
                    "Successfully retrieved history record {RecordId}",
                    id);

                return Ok(record);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("History record {RecordId} not found", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting history record {RecordId}",
                    id);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetAllHistoryRecords()
        {
            _logger.LogDebug("Getting all history records");

            try
            {
                var records = await _taskHistoryService.GetAllAsync();

                _logger.LogDebug(
                    "Retrieved {Count} history records",
                    records.Count());

                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting all history records");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetHistoryForTask(string taskId)
        {
            _logger.LogDebug(
                "Getting history records for task {TaskId}",
                taskId);

            try
            {
                var records = await _taskHistoryService.GetByTaskIdAsync(taskId);

                _logger.LogDebug(
                    "Retrieved {Count} history records for task {TaskId}",
                    records.Count(), taskId);

                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting history for task {TaskId}",
                    taskId);
                return BadRequest(ex.Message);
            }
        }
    }
}