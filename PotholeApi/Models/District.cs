    using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace PotholeApi.Models
{
    public class District
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public required string Name { get; set; }
        public MultiPolygon? Geometry { get; set; }
        public ICollection<Neighbourhood>? Neighbourhoods { get; set; }
    }
}
