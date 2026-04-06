using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;
using static PotholeApi.Models.Enums;

namespace PotholeApi.Models
{
    //[Index(nameof(Latitude), nameof(Longitude))]
    public class Pothole
    {
        public int Id { get; set; }
        //public double Latitude { get; set; }
        //public double Longitude { get; set; }
        public Point? Location { get; set; }
        [NotMapped]
        public double Latitude => Location?.Y ?? 0; 

        [NotMapped]
        public double Longitude => Location?.X ?? 0;
        public string? Description { get; set; }
        public Level LevelOfSeverity { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime ReportedAt { get; set; }
        public int? NeighbourhoodId { get; set; }
        public Neighbourhood? Neighbourhood { get; set; }
        public double? confidence { get; set; }
        //public Polygon? BoundaryBox { get; set; }
}
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}
