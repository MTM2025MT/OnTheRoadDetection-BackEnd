namespace PotholeApi.Models
{


    public class Street
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int NeighbourhoodId { get; set; }
        public Neighbourhood? Neighbourhood { get; set; }
    }
}
