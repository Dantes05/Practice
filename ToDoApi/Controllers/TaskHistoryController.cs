using Application.DTOs;
using Application.ServicesInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ToDoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/history")]
    public class TaskHistoryController : ControllerBase
    {
        private readonly ITaskHistoryService _taskHistoryService;

        public TaskHistoryController(ITaskHistoryService taskHistoryService)
        {
            _taskHistoryService = taskHistoryService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskHistoryDto>> GetHistoryRecord(string id)
        {
            try
            {
                var record = await _taskHistoryService.GetByIdAsync(id);
                return Ok(record);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetAllHistoryRecords()
        {
            try
            {
                var records = await _taskHistoryService.GetAllAsync();
                return Ok(records);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetHistoryForTask(string taskId)
        {
            try
            {
                var records = await _taskHistoryService.GetByTaskIdAsync(taskId);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
