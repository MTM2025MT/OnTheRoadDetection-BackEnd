using NetTopologySuite.Geometries;

namespace PotholeApi.Models
{
    public class Neighbourhood
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public MultiPolygon? Geometry { get; set; }
        public int? DistrictId { get; set; }
        public District? District { get; set; }
        public List<Pothole>? Potholes { get; set; }
    }
}
