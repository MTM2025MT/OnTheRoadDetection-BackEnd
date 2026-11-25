using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PotholeApi.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PotholeApi.Services
{
    public class DetectionDatabaseWorker : BackgroundService
    {
        private readonly DetectionQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DetectionDatabaseWorker> _logger;

        public DetectionDatabaseWorker(DetectionQueue queue, IServiceProvider serviceProvider, ILogger<DetectionDatabaseWorker> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int BATCH_SIZE = 100;
            var batch = new List<Pothole>();

            await foreach (var payload in _queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    string ImageUrl = "";

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // Resolve scoped services safely
                        var photoService = scope.ServiceProvider.GetRequiredService<IPhotoService>();
                        var imageRes = await photoService.UploadPhotoWithResultAsBytesAsync(payload.Image);
                        if (imageRes != null) ImageUrl = imageRes.Url?.ToString() ?? "";

                        // Optionally resolve the repository or DbContext for saving
                        var dbContext = scope.ServiceProvider.GetRequiredService<Context>();

                        // Build and add pothole to batch
                        batch.Add(new Pothole
                        {
                            Location = new NetTopologySuite.Geometries.Point(
                                payload.Location.Longitude,
                                payload.Location.Latitude
                            )
                            { SRID = 4326 },
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

                        if (batch.Count >= BATCH_SIZE)
                        {
                            dbContext.Potholes.AddRange(batch);
                            await dbContext.SaveChangesAsync(stoppingToken);
                            batch.Clear();
                            _logger.LogInformation("Saved batch of potholes");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing pothole detection");
                }
            }

            if (batch.Count > 0)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
                dbContext.Potholes.AddRange(batch);
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation($"Saved final batch of {batch.Count} potholes");
            }
        }
    }
}
