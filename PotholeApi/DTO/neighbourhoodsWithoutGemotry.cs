using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using PotholeApi.Models;

namespace PotholeApi.DTO
{
    public class neighbourhoodsWithoutGemotry 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DistrictId { get; set; }
    }
}
