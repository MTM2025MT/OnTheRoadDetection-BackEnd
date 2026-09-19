using Microsoft.AspNetCore.Http;
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
        public IActionResult GetJobTask(int id)
        {
            try
            {
                var jobTask = _jobTaskRepository.GetByIdAsync(id).Result;
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
        public IActionResult GetAllJobTasks()
        {
            try
            {
                var jobTasks = _jobTaskRepository.GetAllAsync().Result;
                return Ok(jobTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllJobTasks: error fetching job tasks");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobTask([FromBody] JobTaskcreate JobTaskcreate)
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
                    PotholeId = JobTaskcreate.PotholeId,
                    OfficerId = JobTaskcreate.OfficerId,
                    RepairTeamId = JobTaskcreate.RepairTeamId,
                    AssignedAt = DateTime.UtcNow,
                    Notes = JobTaskcreate.Notes,
                    Priority = JobTaskcreate.Priority,
                    Budget = JobTaskcreate.Budget,
                };
                var createdJobTask = await _jobTaskRepository.AddAsync(jobTask);
                return CreatedAtAction(nameof(GetJobTask), new { id = createdJobTask.Id }, createdJobTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateJobTask: error creating job task for pothole {PotholeId}", JobTaskcreate.PotholeId);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteJobTask(int id)
        {
            try
            {
                var existingJobTask = _jobTaskRepository.GetByIdAsync(id).Result;
                if (existingJobTask == null)
                {
                    _logger.LogWarning("DeleteJobTask: job task {JobTaskId} not found", id);
                    return NotFound();
                }
                _jobTaskRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteJobTask: error deleting job task {JobTaskId}", id);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateJobTask(int id, [FromBody] JobTask jobTask)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var existingJobTask = _jobTaskRepository.GetByIdAsync(id).Result;
                if (existingJobTask == null)
                {
                    _logger.LogWarning("UpdateJobTask: job task {JobTaskId} not found", id);
                    return NotFound();
                }
                jobTask.Id = id;
                _jobTaskRepository.UpdateAsync(jobTask);
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
