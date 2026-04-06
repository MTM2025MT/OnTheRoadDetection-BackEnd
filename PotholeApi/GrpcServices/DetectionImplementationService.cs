using Azure.Core;
using Grpc.Core;
using PotholeApi.Models;
using PotholeApi.Protos;
using PotholeApi.Services;
namespace PotholeApi.GrpcServices
{
    public class DetectionImplementationService : DetectionService.DetectionServiceBase
    {
        //AddData (stream PotholeRequest) returns (StreamSummary);

        
         private readonly DetectionQueue _queue;
        private readonly PhotoService _photoService;
        private readonly ILogger _logger;
        // Inject the queue
        public DetectionImplementationService(DetectionQueue queue, PhotoService PhotoService, ILogger logger)
        {
            _queue = queue;
            _photoService = PhotoService;
            _logger = logger;
        }
        /*
            public override async Task<StreamSummary> StreamPotholes(
                IAsyncStreamReader<PotholeData> requestStream, 
                ServerCallContext context)
            {
                int count = 0;

                await foreach (var request in requestStream.ReadAllAsync())
                {
                    var payload = new PotholeProcessingPayload(
                        request.LocationId,
                        request.Confidence,
                        request.ImageData.ToByteArray(),
                        DateTime.UtcNow
                    );

                    // ⚡ Drop it in the queue and instantly move on!
                    await _queue.QueuePotholeAsync(payload);
                    count++;
                }

                return new StreamSummary { TotalPotholesReceived = count, Success = true };
            }*/
        public override async Task<StreamSummary> AddData(IAsyncStreamReader<PotholeRequest> requestStream, ServerCallContext context)
        {
            // Implement your logic to handle the incoming stream of PotholeRequest messages
            // and return a StreamSummary response.
            // For demonstration purposes, we'll just return a dummy StreamSummary.
            int count = 0;
            await foreach (var pothole in requestStream.ReadAllAsync())
            {

                _logger.LogInformation ($"Received pothole at {pothole.Location} with {pothole.Confidence} confidence.");
                var payload = new DetectionQueueItem
                {
                    Location = new PotholeApi.Models.Location
                    {
                        Latitude = pothole.Location.Latitude,
                        Longitude = pothole.Location.Longitude
                    },
                    Description = pothole.Description,
                    LevelOfSeverity = pothole.LevelOfSeverity,
                    Image = pothole.Image.ToArray(),
                    BoundingBoxesArrays = pothole.BoundingBoxesArrays.Select(arr => arr.Values.ToArray()).ToList(),
                    Confidence = pothole.Confidence,
                };

                // ⚡ Drop it in the queue and instantly move on!
                await _queue.QueuePotholeAsync(payload);
                count++;
                // You could save the image to disk or a database here

                count++;
            }

            // Once the client says "I'm done", the loop exits and we return the summary
            return new StreamSummary
            {
                TotalPotholesReceived = count,
                Success = true
            };
            
        }
    }
}
