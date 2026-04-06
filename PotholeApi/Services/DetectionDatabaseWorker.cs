using PotholeApi.Models;
using PotholeApi.Repositories;

namespace PotholeApi.Services
{
    public class DetectionDatabaseWorker: BackgroundService
    {
        private readonly DetectionQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DetectionDatabaseWorker> _logger;
        private readonly PotholeRepository _potholeRepository;
        private readonly PhotoService _photoService;
        public DetectionDatabaseWorker(PhotoService photoService, DetectionQueue queue, IServiceProvider serviceProvider, ILogger<DetectionDatabaseWorker> logger, PotholeRepository PotholeRepository)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _potholeRepository = PotholeRepository;
            _photoService = photoService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int BATCH_SIZE = 100; // Adjust based on your needs
            var batch = new List<Pothole>();
            
            await foreach (var payload in _queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    // Upload image
                    var ImageUrl = "";
                    var ImageRes = await _photoService.UploadPhotoWithResultAsBytesAsync(payload.Image);
                    if (ImageRes != null)
                    {
                        ImageUrl = ImageRes.Url?.ToString() ?? "";
                    }

                    // Add to batch
                    batch.Add(new Pothole
                    {
                        Location = new NetTopologySuite.Geometries.Point(
                            payload.Location.Longitude,
                            payload.Location.Latitude
                        ) { SRID = 4326 },
                        ReportedAt = DateTime.UtcNow,
                        LevelOfSeverity = payload.LevelOfSeverity switch
                        {
                            >= 7 => Enums.Level.High,
                            >= 4 => Enums.Level.Medium,
                            _ => Enums.Level.Low,
                        },
                        confidence = payload.Confidence,
                        Description = payload.Description,
                        ImageUrl = ImageUrl
                    });

                    // Save batch when full
                    if (batch.Count >= BATCH_SIZE)
                    {
                        await SaveBatchAsync(batch, stoppingToken);
                        batch.Clear();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing pothole detection");
                }
            }
            
            // Save any remaining items
            if (batch.Count > 0)
            {
                await SaveBatchAsync(batch, stoppingToken);
            }
        }

        private async Task SaveBatchAsync(List<Pothole> potholes, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
            
            dbContext.Potholes.AddRange(potholes);
            await dbContext.SaveChangesAsync(stoppingToken);
            
            _logger.LogInformation($"Saved batch of {potholes.Count} potholes to database");
        }
    }
}
