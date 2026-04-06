using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using System.Collections.ObjectModel;

namespace PotholeApi.Repositories
{
    public class PotholeRepository : IPotholeRepository
    {
        private readonly Context _context;

        public PotholeRepository(Context context)
        {
            _context = context;
        }

        public async Task<Pothole> CreatePotholeAsync(Pothole pothole)
        {
            // FIX: If Latitude/Longitude properties are gone, we MUST require Location here.
            if (pothole.Location == null)
            {
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
            return pothole;
        }

        public async Task<Pothole?> UpdatePotholeAsync(int id, Pothole pothole)
        {
            var potholeToUpdate = await _context.Potholes.FindAsync(id);
            if (potholeToUpdate == null)
            {
                return null;
            }

            potholeToUpdate.Description = pothole.Description;
            potholeToUpdate.ImageUrl = pothole.ImageUrl;
            potholeToUpdate.LevelOfSeverity = pothole.LevelOfSeverity;

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
            if (pothole == null) return false;

            _context.Potholes.Remove(pothole);
            await _context.SaveChangesAsync();
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
                throw new KeyNotFoundException($"Pothole with ID {id} does not exist.");
            }
        }
    }
}