using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PotholeApi.DTO;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using PotholeApi.Repositories;
using PotholeApi.Services;
using PotholeApi.Services.DashboardsServices;
using System.Security.Claims;
using System.Threading.Tasks;
using static PotholeApi.Models.Enums;
namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Officer")]
    public class PotholeController : ControllerBase
    {
        IPotholeRepository _potholeRepository;
        IJobTaskRepository _jobTaskRepository;
        IOfficerRepository _officerRepository;
        IPhotoService _photoService;
        IPotholeDashboardsService _potholeDashboardsService;
        private readonly ILogger<PotholeController> _logger;
        public PotholeController(IOfficerRepository officerRepository, IPotholeDashboardsService potholeDashboardsService, IPotholeRepository potholeRepository, IPhotoService photoService, IJobTaskRepository jobTaskRepository, ILogger<PotholeController> logger)
        {
            _potholeRepository = potholeRepository;
            _photoService = photoService;
            _jobTaskRepository = jobTaskRepository;
            _potholeDashboardsService = potholeDashboardsService;
            _officerRepository = officerRepository;
            _logger = logger;
        }
        [HttpGet("{id}")]
        public IActionResult GetPothole(int id)
        {
            try
            {
                var pothole = _potholeRepository.GetPotholeByIdAsync(id).Result;
                if (pothole == null)
                {
                    _logger.LogWarning("GetPothole: pothole {PotholeId} not found", id);
                    return NotFound();
                }
                return Ok(pothole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPothole: error fetching pothole {PotholeId}", id);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        [HttpGet("GetAll")]
        public IActionResult GetAllPotholes()
        {
            try
            {
                var Potholes = _potholeRepository.GetAllPotholesAsync().Result;
                return Ok(Potholes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllPotholes: error fetching potholes");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpGet("bounds")]
        public async Task<IActionResult> GetPotholesByBounds(double first_latitude, double first_longitude, double second_latitude, double second_longitude)
        {
            try
            {
                if (first_latitude == 0 ||
                    first_longitude == 0 ||
                    second_latitude == 0 ||
                    second_longitude == 0)
                {
                    return BadRequest("All bounds parameters are required and must be non-zero.");
                }
                var Potholes = await _potholeRepository.GetPotholesbyRange(first_latitude, first_longitude, second_latitude, second_longitude);

                return Ok(Potholes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPotholesByBounds: error fetching potholes in range");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreatePothole([FromForm] CreatePotholePost Recivedpothole)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                string imageUrl = "";
                if (Recivedpothole.Photo != null)
                {
                    var photoResult = await _photoService.UploadPhotoWithResultAsync(Recivedpothole.Photo);
                    imageUrl = photoResult.Url.ToString();

                }
                var location = new Point(Recivedpothole.Longitude, Recivedpothole.Latitude) { SRID = 4326 };
                var newPothole = new Pothole
                {
                   Location = location,
                    Description = Recivedpothole.Description,
                    ReportedAt = DateTime.UtcNow,
                    ImageUrl = imageUrl,
                    LevelOfSeverity = Enums.Level.Medium

                };
                var potholeitem = _potholeRepository.CreatePotholeAsync(newPothole).Result;
                _logger.LogInformation("Created pothole {PotholeId} via manual report", potholeitem.Id);
                return StatusCode(201, potholeitem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreatePothole: error creating pothole");
                return StatusCode(500, "Internal server error: " + ex.Message + "  " + ex.InnerException);
            }

        }

        // Bu metodu CreateWithFotoAsUrl'nin altına veya sınıfın içine bir yere yapıştır
        private IFormFile ConvertBase64ToFormFile(string base64String, string fileName)
        {
            // Base64 başlıklarını temizle (varsa) örn: "data:image/jpeg;base64,"
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            byte[] bytes = Convert.FromBase64String(base64String);
            var stream = new MemoryStream(bytes);

            // FormFile oluşturuyoruz
            var file = new FormFile(stream, 0, bytes.Length, "image_file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg" // Resim formatına göre değişebilir
            };

            return file;
        }
        [HttpPost("CreateWithFotoAsUrl")]
        public async Task<IActionResult> CreatePothole([FromBody] DetectionData receivedJson)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateWithFotoAsUrl: invalid model state: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                //#region creatingBonderyBoxForPothole
                //var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                //var coordinates = receivedJson.BoundingBox?
                // .Select(pointArray => new Coordinate(pointArray[0], pointArray[1]))
                //    .ToList();
                //if (coordinates?.Count < 3)
                //{
                //    return BadRequest("A pothole boundary must have at least 3 points.");
                //}
                //if (!coordinates.First().Equals2D(coordinates.Last()))
                //{
                //    coordinates.Add(coordinates.First());
                //}

                //// 5. Create the Polygon
                //var potholePolygon = geometryFactory.CreatePolygon(coordinates.ToArray());

                //#endregion

                string finalImageUrl = "";
                if (!string.IsNullOrEmpty(receivedJson.ImageBase64))
                {
                    // 1. Base64 string'i IFormFile'a çeviriyoruz
                    var formFile = ConvertBase64ToFormFile(receivedJson.ImageBase64, "upload.jpg");
                    if (formFile == null)
                    {
                        return BadRequest("Failed to process image. The Base64 string is invalid or corrupted.");
                    }
                    // 2. Artık elinde IFormFile var, servise gönderebilirsin
                    var photoResult = await _photoService.UploadPhotoWithResultAsync(formFile);

                    // 3. Cloudinary/Servis'ten dönen gerçek URL'i alıyoruz
                    if (photoResult.Url != null)
                    {
                        finalImageUrl = photoResult.Url.ToString();
                    }
                }
                // 3. Map the data from the JSON to your database entity  MatchedGps
                var location = new Point(receivedJson.MatchedGps.Longitude, receivedJson.MatchedGps.Latitude) { SRID = 4326 };
                var newPothole = new Pothole
                {
                    // Get coordinates from the nested "matched_gps" object
                    Location = location,

                    // The JSON doesn't have a description, so we add a default one or leave it empty
                    Description = $"AI Detection (Confidence: {receivedJson.Confidence})",

                    // Get the date from the nested "createdAt" object (or use DateTime.UtcNow)
                    ReportedAt = DateTime.UtcNow,

                    // The JSON already contains the URL in "_stored_file", so we don't need to upload it again
                    ImageUrl = finalImageUrl,

                    LevelOfSeverity = receivedJson.LevelOfSeverity switch
                    {
                        >= 7 => Enums.Level.High,
                        >= 4 => Enums.Level.Medium,
                        _ => Enums.Level.Low,
                    }
                };

                // 4. Use 'await' here. Do not use '.Result' (it freezes the server)
                var potholeItem = await _potholeRepository.CreatePotholeAsync(newPothole);

                _logger.LogInformation("Created pothole {PotholeId} from AI detection (confidence {Confidence})", potholeItem.Id, receivedJson.Confidence);
                return StatusCode(201, potholeItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateWithFotoAsUrl: error creating pothole from AI detection");
                return StatusCode(500, "Internal server error: " + ex.Message + " " + ex.InnerException);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePothole(int id)
        {
            try
            {
                var existingPothole = _potholeRepository.GetPotholeByIdAsync(id).Result;
                if (existingPothole == null)
                {
                    _logger.LogWarning("DeletePothole: pothole {PotholeId} not found", id);
                    return NotFound();
                }
                var photoDeletion = await _photoService.DeletePhotoAsync(existingPothole.ImageUrl);
                var result = _potholeRepository.DeletePotholeAsync(id).Result;
                if (!result)
                {
                    return NotFound();
                }
                _logger.LogInformation("Deleted pothole {PotholeId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeletePothole: error deleting pothole {PotholeId}", id);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }


        }


        [HttpPut("{id}")]
        public IActionResult UpdatePothole(int id, [FromBody] Pothole pothole)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existingPothole = _potholeRepository.GetPotholeByIdAsync(id).Result;
                if (existingPothole == null)
                {
                    _logger.LogWarning("UpdatePothole: pothole {PotholeId} not found", id);
                    return NotFound();
                }
                var updatedPothole = _potholeRepository.UpdatePotholeAsync(id, pothole).Result;
                _logger.LogInformation("Updated pothole {PotholeId}", id);
                return Ok(updatedPothole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePothole: error updating pothole {PotholeId}", id);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }

        [HttpGet("GetByDistrictId/{districtId}")]
        public async Task<IActionResult> GetPotholesByDistrictId(int districtId)
        {
            try
            {
                if (districtId <= 0)
                {
                    return BadRequest("Invalid district ID.");
                }
                var potholes = await _potholeRepository.GetPotholesByDistrictIdAsync(districtId);
                if (potholes == null || potholes.Count == 0)
                {
                    return NotFound();
                }
                return Ok(potholes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPotholesByDistrictId: error fetching potholes for district {DistrictId}", districtId);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        [HttpGet("GetByNeighbourhoodId/{neighbourhoodId}")]

        public async Task<IActionResult> GetPotholesByNeighbourhoodId(int neighbourhoodId)
        {
            try
            {
                if (neighbourhoodId <= 0)
                {
                    return BadRequest("Invalid neighbourhood ID.");
                }
                var potholes = await _potholeRepository.GetPotholesByNeighbourhoodIdAsync(neighbourhoodId);
                if (potholes == null || potholes.Count == 0)
                {
                    return NotFound();
                }
                return Ok(potholes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPotholesByNeighbourhoodId: error fetching potholes for neighbourhood {NeighbourhoodId}", neighbourhoodId);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> GetPotholesByFilter([FromQuery] RepairStatus? status, [FromQuery] Level? severity)
        {
            try
            {
                var potholes = await _potholeRepository.GetPotholesByFilterAsync(status, severity);
                return Ok(potholes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPotholesByFilter: error filtering potholes by status {Status} and severity {Severity}", status, severity);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPatch("{id}/Status")]
        public async Task<IActionResult> UpdatePotholeStatus(int id, [FromBody] RepairStatus status)
        {
            try
            {
                var existingPothole = await _potholeRepository.GetPotholeByIdAsync(id);
                if (existingPothole == null)
                {
                    _logger.LogWarning("UpdatePotholeStatus: pothole {PotholeId} not found", id);
                    return NotFound();
                }

                existingPothole.Status = status;
                var updatedPothole = await _potholeRepository.UpdatePotholeAsync(id, existingPothole);
                _logger.LogInformation("Updated pothole {PotholeId} status to {Status}", id, status);
                return Ok(updatedPothole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePotholeStatus: error updating status for pothole {PotholeId}", id);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("DoesPotholeAssociatedWithAnyJobTask/{potholeId}")]
        public async Task<bool> DoesPotholeAssociatedWithAnyJobTask(int potholeId)
        {
            var doesPotholeExistWithTask = await _jobTaskRepository.PotholeWithTaskExistAsync(potholeId);
            return doesPotholeExistWithTask;
        }

        [HttpGet("DashboardPotholeData")]
        [Authorize]  // Ensure user is authenticated
        public async Task<IActionResult> GetDashboardPotholeData()
        {
            try
            {
                // Get user ID from the JWT token claims
                var officerUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogDebug("GetDashboardPotholeData: requested by user {UserId}", officerUserId);
                // Or if you use a different claim type:
                // var officerUserId = User.FindFirst("sub")?.Value;
                // var officerUserId = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(officerUserId))
                {
                    _logger.LogWarning("GetDashboardPotholeData: no user ID found in token");
                    return Unauthorized("User ID not found in token");
                }
                var officer = await _officerRepository.GetOfficerByUserIdAsync(officerUserId);

                var dashboardData =await _potholeDashboardsService.GetPotholeDashboardsServices(officer.Id);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDashboardPotholeData: error fetching dashboard data");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }



    }
 }
