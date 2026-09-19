using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using System.Collections.ObjectModel;
using static PotholeApi.Models.Enums;

namespace PotholeApi.Repositories
{
    public class PotholeRepository : IPotholeRepository
    {
        private readonly Context _context;
        private readonly ILogger<PotholeRepository> _logger;

        public PotholeRepository(Context context, ILogger<PotholeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Pothole> CreatePotholeAsync(Pothole pothole)
        {
            // FIX: If Latitude/Longitude properties are gone, we MUST require Location here.
            if (pothole.Location == null)
            {
                _logger.LogWarning("CreatePotholeAsync called without a Location");
                throw new ArgumentException("Pothole Location (Point) is required.");
            }

            // Ensure correct SRID (4326 is standard for GPS)
            if (pothole.Location.SRID != 4326)
            {
                pothole.Location.SRID = 4326;
            }

            // 1. Spatial "Point-in-Polygon" Check
            // Find which Neighbourhood contains this pothole
            var neighbourhoodId = await _context.Neighbourhoods
                .Where(n => n.Geometry.Intersects(pothole.Location))
                .Select(n => (int?)n.Id) // Cast to nullable to handle "not found" gracefully
                .FirstOrDefaultAsync();

            if (neighbourhoodId.HasValue)
            {
                pothole.NeighbourhoodId = neighbourhoodId.Value;
            }

            await _context.Potholes.AddAsync(pothole);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created pothole {PotholeId} in neighbourhood {NeighbourhoodId}", pothole.Id, pothole.NeighbourhoodId);
            return pothole;
        }

        public async Task<Pothole?> UpdatePotholeAsync(int id, Pothole pothole)
        {
            var potholeToUpdate = await _context.Potholes.FindAsync(id);
            if (potholeToUpdate == null)
            {
                _logger.LogWarning("UpdatePotholeAsync: pothole {PotholeId} not found", id);
                return null;
            }

            potholeToUpdate.Description = pothole.Description;
            potholeToUpdate.ImageUrl = pothole.ImageUrl;
            potholeToUpdate.LevelOfSeverity = pothole.LevelOfSeverity;
            potholeToUpdate.Status = pothole.Status;

            // FIX: Only update Location if the new one is valid and different
            if (pothole.Location != null && !pothole.Location.Equals(potholeToUpdate.Location))
            {
                potholeToUpdate.Location = pothole.Location;
                if (potholeToUpdate.Location.SRID != 4326) potholeToUpdate.Location.SRID = 4326;

                // Re-calculate Neighbourhood since the location moved
                var neighbourhoodId = await _context.Neighbourhoods
                    .Where(n => n.Geometry.Intersects(potholeToUpdate.Location))
                    .Select(n => (int?)n.Id)
                    .FirstOrDefaultAsync();

                // If found, update it. If not found (moved into ocean?), set to null/0.
                potholeToUpdate.NeighbourhoodId = neighbourhoodId ?? 0;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated pothole {PotholeId}", id);
            return potholeToUpdate;
        }

        public async Task<ICollection<Pothole>> GetPotholesbyRange(double firstLatitude, double firstLongitude, double secondLatitude, double secondLongitude)
        {
            // 1. Get the corners (Min and Max)
            var minLat = Math.Min(firstLatitude, secondLatitude);
            var maxLat = Math.Max(firstLatitude, secondLatitude);
            var minLon = Math.Min(firstLongitude, secondLongitude);
            var maxLon = Math.Max(firstLongitude, secondLongitude);

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            // 2. Define coordinates in COUNTER-CLOCKWISE order
            // Think of it like drawing a box starting from the bottom-left and going right.
            var coordinates = new Coordinate[]
            {
                new Coordinate(minLon, minLat), // 1. Bottom-Left
                new Coordinate(maxLon, minLat), // 2. Bottom-Right  (Move Right)
                new Coordinate(maxLon, maxLat), // 3. Top-Right     (Move Up)
                new Coordinate(minLon, maxLat), // 4. Top-Left      (Move Left)
                new Coordinate(minLon, minLat)  // 5. Bottom-Left   (Close the loop)
            };

            // 3. Create the Polygon
            var bbox = geometryFactory.CreatePolygon(coordinates);

            // 4. Run the Query
            var potholes = await _context.Potholes
                .Where(p => p.Location.Intersects(bbox))
                .ToListAsync();

            return new Collection<Pothole>(potholes);
        }
        // --- Standard Methods (These look good) ---

        public async Task<bool> DeletePotholeAsync(int id)
        {
            var pothole = await _context.Potholes.FindAsync(id);
            if (pothole == null)
            {
                _logger.LogWarning("DeletePotholeAsync: pothole {PotholeId} not found", id);
                return false;
            }

            _context.Potholes.Remove(pothole);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted pothole {PotholeId}", id);
            return true;
        }

        public async Task<ICollection<Pothole>> GetAllPotholesAsync()
        {
            return await _context.Potholes.ToListAsync();
        }

        public Task<Pothole?> GetPotholeByIdAsync(int id)
        {
            var pothole = _context.Potholes.FindAsync(id);
            return pothole.AsTask();
        }

        public async Task<ICollection<Pothole>> GetPotholesByDistrictIdAsync(int districtId)
        {
            return await _context.Potholes
                .Where(p => p.Neighbourhood != null && p.Neighbourhood.DistrictId == districtId)
                .ToListAsync();
        }

        public async Task<ICollection<Pothole>> GetPotholesByNeighbourhoodIdAsync(int neighbourhoodId)
        {
            return await _context.Potholes
                .Where(p => p.NeighbourhoodId == neighbourhoodId)
                .ToListAsync();
        }

        public async Task DoesExist(int id)
        {
            if (!await _context.Potholes.AnyAsync(p => p.Id == id))
            {
                _logger.LogWarning("DoesExist: pothole {PotholeId} does not exist", id);
                throw new KeyNotFoundException($"Pothole with ID {id} does not exist.");
            }
        }

        public async Task<ICollection<Pothole>> GetPotholesByFilterAsync(RepairStatus? status, Level? severity)
        {
            var query = _context.Potholes.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (severity.HasValue)
            {
                query = query.Where(p => p.LevelOfSeverity == severity.Value);
            }

            return await query.ToListAsync();
        }
    }
}