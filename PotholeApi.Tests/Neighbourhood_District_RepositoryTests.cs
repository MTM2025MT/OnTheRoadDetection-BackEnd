using Xunit;
using Assert = Xunit.Assert;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Logging.Abstractions;
using PotholeApi.Models;
using PotholeApi.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotholeApi.Tests
{
    internal class Neighbourhood_District_RepositoryTests
    {
    }
    public class RepositoryTestHelpers
    {
        public static Context CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new Context(options);
        }
    }

    public class NeighbourhoodRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsAllNeighbourhoods()
        {
            // Arrange
            using var context = RepositoryTestHelpers.CreateInMemoryContext("nb_all");
            context.Neighbourhoods.AddRange(
                new Neighbourhood { Id = 1, Name = "Alpha", DistrictId = 100 },
                new Neighbourhood { Id = 2, Name = "Beta", DistrictId = 101 }
            );
            context.SaveChanges();

            var repo = new NeighbourhoodRepository(context, NullLogger<NeighbourhoodRepository>.Instance);

            // Act
            var all = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(all);
            Assert.Equal(2, all.Count);
            Assert.Contains(all, n => n.Name == "Alpha");
            Assert.Contains(all, n => n.Name == "Beta");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectNeighbourhood()
        {
            // Arrange
            using var context = RepositoryTestHelpers.CreateInMemoryContext("nb_by_id");
            context.Neighbourhoods.Add(new Neighbourhood { Id = 10, Name = "Gamma", DistrictId = 200 });
            context.SaveChanges();

            var repo = new NeighbourhoodRepository(context, NullLogger<NeighbourhoodRepository>.Instance);

            // Act
            var n = await repo.GetByIdAsync(10);

            // Assert
            Assert.NotNull(n);
            Assert.Equal(10, n!.Id);
            Assert.Equal("Gamma", n.Name);
        }

        [Fact]
        public async Task GetNeighbourhoodsByDistrictIdAsync_FiltersByDistrict()
        {
            // Arrange
            using var context = RepositoryTestHelpers.CreateInMemoryContext("nb_by_district");
            context.Neighbourhoods.AddRange(
                new Neighbourhood { Id = 21, Name = "N1", DistrictId = 300 },
                new Neighbourhood { Id = 22, Name = "N2", DistrictId = 300 },
                new Neighbourhood { Id = 23, Name = "N3", DistrictId = 301 }
            );
            context.SaveChanges();

            var repo = new NeighbourhoodRepository(context, NullLogger<NeighbourhoodRepository>.Instance);

            // Act
            var list = await repo.GetNeighbourhoodsByDistrictIdAsync(300);

            // Assert
            Assert.NotNull(list);
            Assert.Equal(2, list.Count);
            Assert.All(list, n => Assert.Equal(300, n.DistrictId));
        }
    }

    public class DistrictRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsAllDistricts()
        {
            // Arrange
            using var context = RepositoryTestHelpers.CreateInMemoryContext("districts_all");
            context.Districts.AddRange(
                new District { Id = 1, Name = "D1" },
                new District { Id = 2, Name = "D2" }
            );
            context.SaveChanges();

            var repo = new DistrictRepository(context, NullLogger<DistrictRepository>.Instance);

            // Act
            var all = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(all);
            Assert.Equal(2, all.Count());
            Assert.Contains(all, d => d.Name == "D1");
            Assert.Contains(all, d => d.Name == "D2");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsDistrictWhenExists()
        {
            // Arrange
            using var context = RepositoryTestHelpers.CreateInMemoryContext("district_by_id");
            context.Districts.Add(new District { Id = 7, Name = "TargetDistrict" });
            context.SaveChanges();

            var repo = new DistrictRepository(context, NullLogger<DistrictRepository>.Instance);

            // Act
            var d = await repo.GetByIdAsync(7);

            // Assert
            Assert.NotNull(d);
            Assert.Equal(7, d!.Id);
            Assert.Equal("TargetDistrict", d.Name);
        }

        [Fact]
        public async Task GetDistrictNamesAsync_MapsToDistrictJustText()
        {
            // Arrange
            using var context = RepositoryTestHelpers.CreateInMemoryContext("district_names");
            // Geometry can be null; the repository maps centroid Y/X to 0 when Geometry is null
            context.Districts.Add(new District { Id = 11, Name = "TextDistrict", Geometry = null });
            context.SaveChanges();

            var repo = new DistrictRepository(context, NullLogger<DistrictRepository>.Instance);

            // Act
            var names = await repo.GetDistrictNamesAsync();

            // Assert
            Assert.NotNull(names);
            var item = names.FirstOrDefault(x => x.Id == 11);
            Assert.NotNull(item);
            Assert.Equal("TextDistrict", item!.Name);
            Assert.Equal(0, item.CenterLatitude);
            Assert.Equal(0, item.CenterLongitude);
        }
    }
}
