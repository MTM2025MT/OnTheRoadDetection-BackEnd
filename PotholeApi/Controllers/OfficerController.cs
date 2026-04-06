using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PotholeApi.DTO;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using System.Threading.Tasks;
namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficerController : ControllerBase
    {
        IOfficerRepository _officerRepository;
        Context _context;
        public OfficerController(Context context, IOfficerRepository officerRepository)
        {
            _officerRepository = officerRepository;
            _context = context;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var potholes = _officerRepository.GetAllAsync();
                return Ok(potholes);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var officer = _officerRepository.GetByIdAsync(id);
                if (officer == null)
                {
                    return NotFound();
                }
                return Ok(officer);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/jobtasks")]
        public IActionResult GetOfficerWithJobTasks(int id)
        {
            try
            {
                var officer = _officerRepository.GetOfficerWithJobTasksAsync(id);
                if (officer == null)
                {
                    return NotFound();
                }
                return Ok(officer);
            }
            catch (Exception)
            {
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
        public IActionResult Update(int id, [FromBody] Officer officer)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return BadRequest(ModelState);
                }
                var existingOfficer = _officerRepository.GetByIdAsync(id);
                if (existingOfficer == null)
                {
                    return NotFound();
                }
                officer.Id = id;
                _officerRepository.UpdateAsync(officer);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var existingOfficer = _officerRepository.GetByIdAsync(id);
                if (existingOfficer == null)
                {
                    return NotFound();
                }
                _officerRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("CreateScanTask")]
        public IActionResult CreateScanTask([FromBody] ScanExecutionTaskDto dto)
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

            _context.ScanExecutionTasks.Add(entity);
            _context.SaveChanges();
            return Ok(entity);
        }

        [HttpGet("GetScanTasks")]
        public async Task<IActionResult> GetScanTasks()
        {
            var res = await  _context.ScanExecutionTasks.Include(S=>S.RoutePoints) .ToListAsync();
            return Ok(res);
        }

    }
}
