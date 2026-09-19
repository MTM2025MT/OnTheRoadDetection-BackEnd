using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PotholeApi.Models;
using PotholeApi.Repositories;
using static PotholeApi.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using PotholeApi;


namespace PotholeApi.Tests
{
    public static class TestHelpers
    {
        public static Context CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new Context(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        public static GeometryFactory GeometryFactory() =>
            NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
    }

    public class PotholeRepositoryTests : IDisposable
    {
        private readonly GeometryFactory _gf;

        public PotholeRepositoryTests()
        {
            _gf = TestHelpers.GeometryFactory();
        }

        public void Dispose()
        {
            // nothing to dispose here; each test uses its own in-memory DB
        }

        [Fact]
        public async Task CreatePotholeAsync_SetsNeighbourhoodId_WhenPointInsideNeighbourhood()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var ctx = TestHelpers.CreateContext(dbName);

            // Create a simple square polygon that contains the point (lon=0.5, lat=0.5)
            var coords = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 0),
                new Coordinate(1, 1),
                new Coordinate(0, 1),
                new Coordinate(0, 0)
            };
            var poly = _gf.CreatePolygon(coords);
            var multi = _gf.CreateMultiPolygon(new[] { poly });
            multi.SRID = 4326;

            var neighbourhood = new Neighbourhood { Id = 100, Name = "Inner", Geometry = multi };
            ctx.Neighbourhoods.Add(neighbourhood);
            ctx.SaveChanges();

            var repo = new PotholeRepository(ctx, NullLogger<PotholeRepository>.Instance);

            var pothole = new Pothole
            {
                Description = "Hole",
                ReportedAt = DateTime.UtcNow,
                LevelOfSeverity = Level.Low,
                Location = _gf.CreatePoint(new Coordinate(0.5, 0.5)) // X=lon, Y=lat
            };
            pothole.Location.SRID = 4326;

            // Act
            var created = await repo.CreatePotholeAsync(pothole);

            // Assert
            Assert.NotNull(created);
            Assert.Equal(100, created.NeighbourhoodId);
            var persisted = ctx.Potholes.FirstOrDefault();
            Assert.NotNull(persisted);
            Assert.Equal("Hole", persisted.Description);
        }

        [Fact]
        public async Task CreatePotholeAsync_Throws_WhenLocationNull()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = TestHelpers.CreateContext(dbName);
            var repo = new PotholeRepository(ctx, NullLogger<PotholeRepository>.Instance);

            var pothole = new Pothole
            {
                Description = "NoLoc",
                ReportedAt = DateTime.UtcNow,
                LevelOfSeverity = Level.Medium,
                Location = null
            };

            await Assert.ThrowsAsync<ArgumentException>(() => repo.CreatePotholeAsync(pothole));
        }

        [Fact]
        public async Task UpdatePotholeAsync_UpdatesLocationAndNeighbourhood()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = TestHelpers.CreateContext(dbName);

            // Two neighbourhoods
            var coordsA = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(2, 0),
                new Coordinate(2, 2),
                new Coordinate(0, 2),
                new Coordinate(0, 0)
            };
            var polyA = _gf.CreatePolygon(coordsA);
            var multiA = _gf.CreateMultiPolygon(new[] { polyA });
            multiA.SRID = 4326;
            var nA = new Neighbourhood { Id = 1, Name = "A", Geometry = multiA };

            var coordsB = new[]
            {
                new Coordinate(3, 3),
                new Coordinate(5, 3),
                new Coordinate(5, 5),
                new Coordinate(3, 5),
                new Coordinate(3, 3)
            };
            var polyB = _gf.CreatePolygon(coordsB);
            var multiB = _gf.CreateMultiPolygon(new[] { polyB });
            multiB.SRID = 4326;
            var nB = new Neighbourhood { Id = 2, Name = "B", Geometry = multiB };

            ctx.Neighbourhoods.AddRange(nA, nB);

            // Existing pothole located inside neighbourhood A
            var existing = new Pothole
            {
                Id = 10,
                Description = "Old",
                ReportedAt = DateTime.UtcNow,
                LevelOfSeverity = Level.Low,
                Location = _gf.CreatePoint(new Coordinate(1, 1)),
                NeighbourhoodId = 1
            };
            existing.Location.SRID = 4326;

            ctx.Potholes.Add(existing);
            ctx.SaveChanges();

            var repo = new PotholeRepository(ctx, NullLogger<PotholeRepository>.Instance);

            // New values: move to neighbourhood B
            var updatedPothole = new Pothole
            {
                Description = "Updated",
                ReportedAt = existing.ReportedAt,
                LevelOfSeverity = Level.High,
                Location = _gf.CreatePoint(new Coordinate(4, 4)) // inside B
            };
            updatedPothole.Location.SRID = 4326;

            // Act
            var result = await repo.UpdatePotholeAsync(10, updatedPothole);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result!.Description);
            Assert.Equal(2, result.NeighbourhoodId);
        }

        [Fact]
        public async Task GetPotholesbyRange_ReturnsPotholesWithinBBox()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = TestHelpers.CreateContext(dbName);

            var p1 = new Pothole { Id = 1, Description = "In", ReportedAt = DateTime.UtcNow, LevelOfSeverity = Level.Low, Location = _gf.CreatePoint(new Coordinate(1, 1)) };
            p1.Location.SRID = 4326;
            var p2 = new Pothole { Id = 2, Description = "Out", ReportedAt = DateTime.UtcNow, LevelOfSeverity = Level.Low, Location = _gf.CreatePoint(new Coordinate(10, 10)) };
            p2.Location.SRID = 4326;

            ctx.Potholes.AddRange(p1, p2);
            ctx.SaveChanges();

            var repo = new PotholeRepository(ctx, NullLogger<PotholeRepository>.Instance);

            // Box from (0,0) to (2,2) should include p1 only
            var results = await repo.GetPotholesbyRange(0, 0, 2, 2);

            Assert.Single(results);
            Assert.Equal("In", results.First().Description);
        }

        [Fact]
        public async Task DeletePotholeAsync_RemovesAndReturnsTrue_WhenExists()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = TestHelpers.CreateContext(dbName);

            var p = new Pothole { Id = 55, Description = "Del", ReportedAt = DateTime.UtcNow, LevelOfSeverity = Level.Low, Location = _gf.CreatePoint(new Coordinate(0, 0)) };
            p.Location.SRID = 4326;
            ctx.Potholes.Add(p);
            ctx.SaveChanges();

            var repo = new PotholeRepository(ctx, NullLogger<PotholeRepository>.Instance);

            var result = await repo.DeletePotholeAsync(55);

            Assert.True(result);
            Assert.False(ctx.Potholes.Any(x => x.Id == 55));
        }

        [Fact]
        public async Task DoesExist_Throws_KeyNotFound_WhenMissing()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = TestHelpers.CreateContext(dbName);

            var repo = new PotholeRepository(ctx, NullLogger<PotholeRepository>.Instance);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.DoesExist(999));
        }
    }
}