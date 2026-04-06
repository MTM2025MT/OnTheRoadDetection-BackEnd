using NetTopologySuite.Geometries;

namespace PotholeApi.DTO
{
    public class NeighbourhoodWithCenterValues
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DistrictId { get; set; }
        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }
        public MultiPolygon? Geometry { get; set; }
    }
}
