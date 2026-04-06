using Microsoft.EntityFrameworkCore;
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

        public DistrictRepository(Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<District>> GetAllAsync()
        {
            return await _context.Districts.ToListAsync();
        }

        public async Task<District?> GetByIdAsync(int id)
        {
            return await _context.Districts.FindAsync(id);
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