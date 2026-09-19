using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistrictController : ControllerBase
    {
        private readonly IDistrictRepository _repository;
        private readonly ILogger<DistrictController> _logger;

        // Inject the Repository, NOT the Context
        public DistrictController(IDistrictRepository repository, ILogger<DistrictController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET: api/districts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<District>> GetDistrict(int id)
        {
            var district = await _repository.GetByIdAsync(id);

            if (district == null)
            {
                _logger.LogWarning("GetDistrict: district {DistrictId} not found", id);
                return NotFound();
            }

            return Ok(district);
        }

        // GET: api/districts
        [HttpGet]
        public async Task<ActionResult<ICollection<District>>> GetDistricts()
        {
            var districts = await _repository.GetAllAsync();
            return Ok(districts);
        }
        [HttpGet("GetAllWithName")]
        public async Task<ActionResult<ICollection<DistrictJustText>>> GetNamesOfDistrict()
        {
            var district = await _repository.GetDistrictNamesAsync();
            if (district == null)
            {
                return NotFound();
            }
            return Ok(district);
        }
    }
}