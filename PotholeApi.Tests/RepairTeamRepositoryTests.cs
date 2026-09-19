using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PotholeApi.Models;
using PotholeApi.Repositories;



namespace PotholeApi.Tests.Repositories
{
    internal static class RepairTeamTestHelpers
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
    }

    public class RepairTeamRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsAllTeams()
        {
            using var ctx = RepairTeamTestHelpers.CreateContext(Guid.NewGuid().ToString());
            ctx.RepairTeams.AddRange(
                new RepairTeam { Id = 1, Name = "TeamA", ContactEmail = "a@x.com", ContactPhone = "111", Notes = "NotesA" },
                new RepairTeam { Id = 2, Name = "TeamB", ContactEmail = "b@x.com", ContactPhone = "222", Notes = "NotesB" }
            );
            ctx.SaveChanges();

            var repo = new RepairTeamRepository(ctx, NullLogger<RepairTeamRepository>.Instance);

            var all = await repo.GetAllAsync();

            Assert.NotNull(all);
            Assert.Equal(2, all.Count());
            Assert.Contains(all, t => t.Name == "TeamA");
            Assert.Contains(all, t => t.Name == "TeamB");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTeam()
        {
            using var ctx = RepairTeamTestHelpers.CreateContext(Guid.NewGuid().ToString());
            ctx.RepairTeams.Add(new RepairTeam { Id = 10, Name = "Solo", ContactEmail = "s@x.com", ContactPhone = "000", Notes = "NotesSolo" });
            ctx.SaveChanges();

            var repo = new RepairTeamRepository(ctx, NullLogger<RepairTeamRepository>.Instance);

            var t = await repo.GetByIdAsync(10);

            Assert.NotNull(t);
            Assert.Equal(10, t!.Id);
            Assert.Equal("Solo", t.Name);
        }

        [Fact]
        public async Task GetRepairTeamWithTasksAsync_IncludesTasks()
        {
            using var ctx = RepairTeamTestHelpers.CreateContext(Guid.NewGuid().ToString());

            var team = new RepairTeam { Id = 50, Name = "Ops", ContactEmail = "ops@x.com", ContactPhone = "555", Notes = "Ops notes" };
            ctx.RepairTeams.Add(team);

            // Create a job task linked to the team
            ctx.JobTasks.Add(new JobTask { Id = 900, RepairTeamId = 50, OfficerId = 1, PotholeId = 1, AssignedAt = DateTime.UtcNow });
            ctx.SaveChanges();

            var repo = new RepairTeamRepository(ctx, NullLogger<RepairTeamRepository>.Instance);

            var result = await repo.GetRepairTeamWithTasksAsync(50);

            Assert.NotNull(result);
            Assert.Equal(50, result!.Id);
            Assert.NotNull(result.Tasks);
            Assert.Single(result.Tasks);
            Assert.Equal(900, result.Tasks.First().Id);
        }

        [Fact]
        public async Task AddUpdateDelete_Workflow()
        {
            using var ctx = RepairTeamTestHelpers.CreateContext(Guid.NewGuid().ToString());
            var repo = new RepairTeamRepository(ctx, NullLogger<RepairTeamRepository>.Instance);

            var team = new RepairTeam { Name = "NewTeam", ContactEmail = "n@x.com", ContactPhone = "123", Notes = "New team notes" };
            await repo.AddAsync(team);

            Assert.True(ctx.RepairTeams.Any(t => t.Name == "NewTeam"));

            var inserted = ctx.RepairTeams.First();
            inserted.Name = "EditedTeam";
            await repo.UpdateAsync(inserted);

            Assert.True(ctx.RepairTeams.Any(t => t.Name == "EditedTeam"));

            var id = inserted.Id;
            await repo.DeleteAsync(id);

            Assert.False(ctx.RepairTeams.Any(t => t.Id == id));
        }

        [Fact]
        public async Task DoesExist_Throws_WhenMissing()
        {
            using var ctx = RepairTeamTestHelpers.CreateContext(Guid.NewGuid().ToString());
            var repo = new RepairTeamRepository(ctx, NullLogger<RepairTeamRepository>.Instance);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.DoesExist(12345));
        }
    }
}