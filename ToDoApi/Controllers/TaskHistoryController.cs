using Application.DTOs;
using Application.ServicesInterfaces;
using Infrastructure.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace ToDoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/history")]
    [ServiceFilter(typeof(ValidationFilter))]
    public class TaskHistoryController : ControllerBase
    {
        private readonly ITaskHistoryService _taskHistoryService;

        public TaskHistoryController(ITaskHistoryService taskHistoryService)
        {
            _taskHistoryService = taskHistoryService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskHistoryDto>> GetHistoryRecord(
            string id,
            CancellationToken cancellationToken = default)
        {
            var record = await _taskHistoryService.GetByIdAsync(id, cancellationToken);
            return Ok(record);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetAllHistoryRecords(
            CancellationToken cancellationToken = default)
        {
            var records = await _taskHistoryService.GetAllAsync(cancellationToken);
            return Ok(records);
        }

        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetHistoryForTask(
            string taskId,
            CancellationToken cancellationToken = default)
        {
            var records = await _taskHistoryService.GetByTaskIdAsync(taskId, cancellationToken);
            return Ok(records);
        }
    }
}