using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PotholeApi.Models;
using PotholeApi.DTO;
using PotholeApi.IRepositories;
namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobTaskController : ControllerBase
    {
        private IJobTaskRepository _jobTaskRepository;
        public JobTaskController(IJobTaskRepository jobTaskRepository)
        {
            _jobTaskRepository = jobTaskRepository;
        }
        [HttpGet("{id}")]
        public IActionResult GetJobTask(int id)
        {
            try
            {
                var jobTask = _jobTaskRepository.GetByIdAsync(id).Result;
                if (jobTask == null)
                {
                    return NotFound();
                }
                return Ok(jobTask);
            }
            catch (Exception ex)
            {
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
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateJobTask([FromBody] JobTaskcreate JobTaskcreate)
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
                var createdJobTask = _jobTaskRepository.AddAsync(jobTask);
                return CreatedAtAction(nameof(GetJobTask), new { id = createdJobTask.Id }, createdJobTask);
            }
            catch (Exception ex)
            {
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
                    return NotFound();
                }
                _jobTaskRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
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
                    return NotFound();
                }
                jobTask.Id = id;
                _jobTaskRepository.UpdateAsync(jobTask);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
