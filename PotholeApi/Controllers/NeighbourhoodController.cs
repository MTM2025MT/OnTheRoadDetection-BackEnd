using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PotholeApi.DTO;
using PotholeApi.IRepositories;
using PotholeApi.Models;

namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NeighbourhoodController : ControllerBase
    {
        private readonly INeighbourhoodRepository _repository;
        private readonly ILogger<NeighbourhoodController> _logger;
        public NeighbourhoodController(INeighbourhoodRepository repository, ILogger<NeighbourhoodController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNeighbourhoodById(int id)
        {
            var neighbourhood = await _repository.GetByIdAsync(id);
            if (neighbourhood == null)
            {
                _logger.LogWarning("GetNeighbourhoodById: neighbourhood {NeighbourhoodId} not found", id);
                return NotFound();
            }
            var neighbourhoodWithCenterValues = new NeighbourhoodWithCenterValues
            {
                Id = neighbourhood.Id,
                Name = neighbourhood.Name,
                DistrictId = neighbourhood.DistrictId,
                CenterLatitude = neighbourhood.Geometry != null ? neighbourhood.Geometry.Centroid.Y : 0,
                CenterLongitude = neighbourhood.Geometry != null ? neighbourhood.Geometry.Centroid.X : 0,
                Geometry = neighbourhood.Geometry
            };

            return Ok(neighbourhoodWithCenterValues);
        }

        [HttpGet("GetNeighbourhoodByDistrictId/{id}")]
        public async Task<IActionResult> GetNeighbourhoodByDistrictId(int id)
        {
            var neighbourhoods = await _repository.GetNeighbourhoodsByDistrictIdAsync(id);
            if (neighbourhoods == null || neighbourhoods.Count == 0)
            {
                _logger.LogWarning("GetNeighbourhoodByDistrictId: no neighbourhoods found for district {DistrictId}", id);
                return NotFound();
            }
            var neighbourhoodsList = neighbourhoods.Select(n => new NeighbourhoodWithoutGeometry
            {
                Id = n.Id,
                Name = n.Name,
                DistrictId = n.DistrictId
            }).ToList();
            return Ok(neighbourhoodsList);
        }



    }
}
