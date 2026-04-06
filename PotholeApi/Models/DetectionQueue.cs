using System.Threading.Channels;

namespace PotholeApi.Models
{
    /*


  google.protobuf.Timestamp timestamp = 4;
  bytes image = 5;                // binary image data (jpeg/png)
  repeated DoubleArray bounding_boxes_arrays = 6;
  double confidence = 7;
     */

    public record DetectionQueueItem
    {
        
        public required Location Location { get; set; }
        public string? Description { get; set; }
        public byte[] Image { get; set; }
        public List<double[]>? BoundingBoxesArrays { get; set; }
        public int LevelOfSeverity { get; set; }
        public double Confidence { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class Location
    {
          public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    public class DetectionQueue
    {
        private readonly Channel<DetectionQueueItem> _channel =
        Channel.CreateBounded<DetectionQueueItem>(10000);
        public async ValueTask QueuePotholeAsync(DetectionQueueItem payload)
        {
            await _channel.Writer.WriteAsync(payload);
        }

        public IAsyncEnumerable<DetectionQueueItem> ReadAllAsync(CancellationToken ct)
        {
            return _channel.Reader.ReadAllAsync(ct);
        }

    }
}
