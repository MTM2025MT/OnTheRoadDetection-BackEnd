using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PotholeApi.Models;
using PotholeApi.DTO;
using PotholeApi.IRepositories;
using System.Threading.Tasks;
namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobTaskController : ControllerBase
    {
        private IJobTaskRepository _jobTaskRepository;
        private readonly ILogger<JobTaskController> _logger;
        public JobTaskController(IJobTaskRepository jobTaskRepository, ILogger<JobTaskController> logger)
        {
            _jobTaskRepository = jobTaskRepository;
            _logger = logger;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobTask(int id)
        {
            try
            {
                var jobTask = await _jobTaskRepository.GetByIdAsync(id);
                if (jobTask == null)
                {
                    _logger.LogWarning("GetJobTask: job task {JobTaskId} not found", id);
                    return NotFound();
                }
                return Ok(jobTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetJobTask: error fetching job task {JobTaskId}", id);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAllJobTasks()
        {
            try
            {
                var jobTasks = await _jobTaskRepository.GetAllAsync();
                return Ok(jobTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllJobTasks: error fetching job tasks");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobTask([FromBody] JobTaskCreateDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Extract all error messages into a simple list of strings
                    var errorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    // Return a custom anonymous object
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = errorMessages
                    });
                }
                var jobTask = new JobTask
                {
                    PotholeId = request.PotholeId,
                    OfficerId = request.OfficerId,
                    RepairTeamId = request.RepairTeamId,
                    AssignedAt = DateTime.UtcNow,
                    Notes = request.Notes,
                    Priority = request.Priority,
                    Budget = request.Budget,
                };
                var createdJobTask = await _jobTaskRepository.AddAsync(jobTask);
                return CreatedAtAction(nameof(GetJobTask), new { id = createdJobTask.Id }, createdJobTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateJobTask: error creating job task for pothole {PotholeId}", request.PotholeId);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobTask(int id)
        {
            try
            {
                var existingJobTask = await _jobTaskRepository.GetByIdAsync(id);
                if (existingJobTask == null)
                {
                    _logger.LogWarning("DeleteJobTask: job task {JobTaskId} not found", id);
                    return NotFound();
                }
                await _jobTaskRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteJobTask: error deleting job task {JobTaskId}", id);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobTask(int id, [FromBody] JobTask jobTask)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var existingJobTask = await _jobTaskRepository.GetByIdAsync(id);
                if (existingJobTask == null)
                {
                    _logger.LogWarning("UpdateJobTask: job task {JobTaskId} not found", id);
                    return NotFound();
                }
                jobTask.Id = id;
                await _jobTaskRepository.UpdateAsync(jobTask);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateJobTask: error updating job task {JobTaskId}", id);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
