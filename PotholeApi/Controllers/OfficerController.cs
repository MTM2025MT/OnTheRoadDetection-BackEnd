using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using PotholeApi.DTO;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using PotholeApi.Repositories;
using System.Threading.Tasks;
namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficerController : ControllerBase
    {
        IOfficerRepository _officerRepository;
        IScanExecutionTasksRepository _scanExecutionTasksRepository;
        Context _context;
        private readonly ILogger<OfficerController> _logger;
        public OfficerController(IScanExecutionTasksRepository ScanExecutionTasksRepository, Context context, IOfficerRepository officerRepository, ILogger<OfficerController> logger)
        {
            _officerRepository = officerRepository;
            _context = context;
            _scanExecutionTasksRepository = ScanExecutionTasksRepository;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var potholes = await _officerRepository.GetAllAsync();
                return Ok(potholes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll: error fetching officers");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var officer = await _officerRepository.GetByIdAsync(id);
                if (officer == null)
                {
                    _logger.LogWarning("Get: officer {OfficerId} not found", id);
                    return NotFound();
                }
                return Ok(officer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get: error fetching officer {OfficerId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/jobtasks")]
        public async Task<IActionResult> GetOfficerWithJobTasks(int id)
        {
            try
            {
                var officer = await _officerRepository.GetOfficerWithJobTasksAsync(id);
                if (officer == null)
                {
                    _logger.LogWarning("GetOfficerWithJobTasks: officer {OfficerId} not found", id);
                    return NotFound();
                }
                return Ok(officer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOfficerWithJobTasks: error fetching officer {OfficerId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        //[HttpPost]
        //public IActionResult Create([FromBody] Officer officer)
        //{
        //    try
        //    {
        //        if(ModelState.IsValid == false)
        //        {
        //            return BadRequest(ModelState);
        //        }
        //        _officerRepository.AddAsync(officer);
        //        return CreatedAtAction(nameof(Get), new { id = officer.Id }, officer);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Officer officer)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return BadRequest(ModelState);
                }
                var existingOfficer = await _officerRepository.GetByIdAsync(id);
                if (existingOfficer == null)
                {
                    _logger.LogWarning("Update: officer {OfficerId} not found", id);
                    return NotFound();
                }
                officer.Id = id;
                _officerRepository.UpdateAsync(officer);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update: error updating officer {OfficerId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingOfficer = await _officerRepository.GetByIdAsync(id);
                if (existingOfficer == null)
                {
                    _logger.LogWarning("Delete: officer {OfficerId} not found", id);
                    return NotFound();
                }
                await _officerRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete: error deleting officer {OfficerId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("CreateScanTask")]
        public async Task<IActionResult> CreateScanTask([FromBody] ScanExecutionTaskDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new ScanExecutionTask
            {
                VehicleId = dto.VehicleId,
                StartTime = dto.StartTime,
                EstimatedMinutes = dto.EstimatedMinutes,
                Type = dto.Type,
                IntervalHours = dto.IntervalHours,
                EndTime = dto.EndTime,
                Priority = dto.Priority,
                AutoRescan = dto.AutoRescan,
                SendNotifications = dto.SendNotifications,
                Notes = dto.Notes,
                RoutePoints = dto.RoutePoints.Select(r => new RoutePoint
                {
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    Order = r.Order
                }).ToList(),
                RoutePath = dto.RoutePath.Select(p => new GpsCoordinate
                {
                    Latitude = p.Latitude,
                    Longitude = p.Longitude
                }).ToList()
            };


            await _scanExecutionTasksRepository.AddAsync(entity);
            _logger.LogInformation("Created scan task for vehicle {VehicleId}", dto.VehicleId);
            return Ok(entity);
        }

        [HttpGet("GetScanTasks")]
        public async Task<IActionResult> GetScanTasks()
        {
            var res = await _scanExecutionTasksRepository.GetAllAsync();
            return Ok(res);
        }

    }
}
