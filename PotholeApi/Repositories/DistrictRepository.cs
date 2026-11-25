using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PotholeApi.Repositories
{
    public class DistrictRepository : IDistrictRepository
    {
        private readonly Context _context;
        private readonly ILogger<DistrictRepository> _logger;

        public DistrictRepository(Context context, ILogger<DistrictRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<District>> GetAllAsync()
        {
            return await _context.Districts.ToListAsync();
        }

        public async Task<District?> GetByIdAsync(int id)
        {
            var district = await _context.Districts.FindAsync(id);
            if (district == null)
            {
                _logger.LogWarning("GetByIdAsync: district {DistrictId} not found", id);
            }
            return district;
        }
        public async Task<List<DistrictJustText>> GetDistrictNamesAsync()
        {
            return await _context.Districts
                .Select(d => new DistrictJustText
                {
                    Id = d.Id,
                    Name = d.Name,
                    CenterLatitude = d.Geometry != null ? d.Geometry.Centroid.Y : 0,
                    CenterLongitude = d.Geometry != null ? d.Geometry.Centroid.X : 0
                })
                
                .ToListAsync();
        }
    }
}