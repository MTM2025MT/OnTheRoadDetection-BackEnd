using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
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
            var Neighbourhood = await _repository.GetByIdAsync(id);
            if (Neighbourhood == null)
            {
                _logger.LogWarning("GetNeighbourhoodById: neighbourhood {NeighbourhoodId} not found", id);
                return NotFound();
            }
            var neighbourhoodWithCentervalues = new NeighbourhoodWithCenterValues
            {
                Id = Neighbourhood.Id,
                Name = Neighbourhood.Name,
                DistrictId = Neighbourhood.DistrictId,
                CenterLatitude = Neighbourhood.Geometry != null ? Neighbourhood.Geometry.Centroid.Y : 0,
                CenterLongitude = Neighbourhood.Geometry != null ? Neighbourhood.Geometry.Centroid.X : 0,
                Geometry = Neighbourhood.Geometry 
            };

            if (neighbourhoodWithCentervalues == null)
            {
                return NotFound();
            }

            return Ok(neighbourhoodWithCentervalues);
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
            var neighbourdhoodsList = neighbourhoods.Select(n => new neighbourhoodsWithoutGemotry
            {
                Id = n.Id,
                Name = n.Name,
                DistrictId = n.DistrictId
            }).ToList();
            return Ok(neighbourdhoodsList);
        }



    }
}
