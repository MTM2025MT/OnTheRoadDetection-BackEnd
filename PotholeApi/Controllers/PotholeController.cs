using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin,Officer")]
    public class PotholeController : ControllerBase
    {
        IPotholeRepository _potholeRepository;
        IJobTaskRepository _jobTaskRepository;
        IOfficerRepository _officerRepository;
        IPhotoService _photoService;
        IPotholeDashboardsService _potholeDashboardsService;
        public PotholeController(IOfficerRepository officerRepository, IPotholeDashboardsService potholeDashboardsService, IPotholeRepository potholeRepository, IPhotoService photoService, IJobTaskRepository jobTaskRepository)
        {
            _potholeRepository = potholeRepository;
            _photoService = photoService;
            _jobTaskRepository = jobTaskRepository;
            _potholeDashboardsService = potholeDashboardsService;
            _officerRepository = officerRepository;
        }
        [HttpGet("{id}")]
        public IActionResult GetPothole(int id)
        {
            try
            {
                var pothole = _potholeRepository.GetPotholeByIdAsync(id).Result;
                if (pothole == null)
                {
                    return NotFound();
                }
                return Ok(pothole);
            }
            catch (Exception ex)
            {
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
                return StatusCode(201, potholeitem);
            }
            catch (Exception ex)
            {
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
                Console.WriteLine(ModelState);
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

                return StatusCode(201, potholeItem);
            }
            catch (Exception ex)
            {
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
                    return NotFound();
                }
                var photoDeletion = await _photoService.DeletePhotoAsync(existingPothole.ImageUrl);
                var result = _potholeRepository.DeletePotholeAsync(id).Result;
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
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
                    return NotFound();
                }
                var updatedPothole = _potholeRepository.UpdatePotholeAsync(id, pothole).Result;
                return Ok(updatedPothole);
            }
            catch (Exception ex)
            {
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
                Console.WriteLine("/////////${officerUserId is"+ officerUserId);
                // Or if you use a different claim type:
                // var officerUserId = User.FindFirst("sub")?.Value;
                // var officerUserId = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(officerUserId))
                {
                    return Unauthorized("User ID not found in token");
                }
                var officer = await _officerRepository.GetOfficerByUserIdAsync(officerUserId);

                var dashboardData =await _potholeDashboardsService.GetPotholeDashboardsServices(officer.Id);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }

    }
 }
