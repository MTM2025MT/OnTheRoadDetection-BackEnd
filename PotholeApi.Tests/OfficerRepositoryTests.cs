using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PotholeApi.Models;
using PotholeApi.Repositories;



namespace PotholeApi.Tests.Repositories
{
    internal static class TestHelpers
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

        public static UserManager<ApplicationUser> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>().Object;
            var identityOptions = Options.Create(new IdentityOptions());
            var pwdHasher = new PasswordHasher<ApplicationUser>();
            var userValidators = Array.Empty<IUserValidator<ApplicationUser>>();
            var pwdValidators = Array.Empty<IPasswordValidator<ApplicationUser>>();
            var lookupNormalizer = Mock.Of<ILookupNormalizer>();
            var errors = new IdentityErrorDescriber();
            var services = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<UserManager<ApplicationUser>>>();

            return new UserManager<ApplicationUser>(
                store,
                identityOptions,
                pwdHasher,
                userValidators,
                pwdValidators,
                lookupNormalizer,
                errors,
                services,
                logger
            );
        }
    }

    public class OfficerRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsAllOfficers()
        {
            using var ctx = TestHelpers.CreateContext(Guid.NewGuid().ToString());
            ctx.Officers.AddRange(
                new Officer { Id = 1, BadgeNumber = "B1", DistrictId = 10 },
                new Officer { Id = 2, BadgeNumber = "B2", DistrictId = 11 }
            );
            ctx.SaveChanges();

            var repo = new OfficerRepository(ctx, TestHelpers.CreateMockUserManager(), NullLogger<OfficerRepository>.Instance);

            var all = await repo.GetAllAsync();

            Assert.NotNull(all);
            Assert.Equal(2, all.Count);
            Assert.Contains(all, o => o.BadgeNumber == "B1");
            Assert.Contains(all, o => o.BadgeNumber == "B2");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOfficer()
        {
            using var ctx = TestHelpers.CreateContext(Guid.NewGuid().ToString());
            ctx.Officers.Add(new Officer { Id = 5, BadgeNumber = "Target", DistrictId = 2 });
            ctx.SaveChanges();

            var repo = new OfficerRepository(ctx, TestHelpers.CreateMockUserManager(), NullLogger<OfficerRepository>.Instance);

            var o = await repo.GetByIdAsync(5);

            Assert.NotNull(o);
            Assert.Equal(5, o!.Id);
            Assert.Equal("Target", o.BadgeNumber);
        }

        [Fact]
        public async Task GetOfficerWithJobTasksAsync_IncludesTasks()
        {
            using var ctx = TestHelpers.CreateContext(Guid.NewGuid().ToString());

            var officer = new Officer { Id = 20, BadgeNumber = "JTI", DistrictId = 1 };
            ctx.Officers.Add(officer);

            ctx.JobTasks.Add(new JobTask { Id = 100, OfficerId = 20, PotholeId = 1, RepairTeamId = 1, AssignedAt = DateTime.UtcNow });
            ctx.SaveChanges();

            var repo = new OfficerRepository(ctx, TestHelpers.CreateMockUserManager(), NullLogger<OfficerRepository>.Instance);

            var result = await repo.GetOfficerWithJobTasksAsync(20);

            Assert.NotNull(result);
            Assert.Equal(20, result!.Id);
            Assert.NotNull(result.JobTasks);
            Assert.Single(result.JobTasks);
            Assert.Equal(100, result.JobTasks.First().Id);
        }

        [Fact]
        public async Task AddUpdateDelete_Workflow()
        {
            using var ctx = TestHelpers.CreateContext(Guid.NewGuid().ToString());
            var repo = new OfficerRepository(ctx, TestHelpers.CreateMockUserManager(), NullLogger<OfficerRepository>.Instance);

            var officer = new Officer { BadgeNumber = "NEW", DistrictId = 3 };
            await repo.AddAsync(officer);

            Assert.True(ctx.Officers.Any(o => o.BadgeNumber == "NEW"));

            officer.BadgeNumber = "EDITED";
            await repo.UpdateAsync(officer);

            Assert.True(ctx.Officers.Any(o => o.BadgeNumber == "EDITED"));

            var id = ctx.Officers.First().Id;
            await repo.DeleteAsync(id);

            Assert.False(ctx.Officers.Any(o => o.Id == id));
        }

        [Fact]
        public async Task DoesExist_Throws_WhenMissing()
        {
            using var ctx = TestHelpers.CreateContext(Guid.NewGuid().ToString());
            var repo = new OfficerRepository(ctx, TestHelpers.CreateMockUserManager(), NullLogger<OfficerRepository>.Instance);

            await Assert.ThrowsAsync<Exception>(() => repo.DoesExist(9999));
        }

        [Fact]
        public async Task GetOfficerByUserIdAsync_ReturnsOfficer_WhenFound()
        {
            using var ctx = TestHelpers.CreateContext(Guid.NewGuid().ToString());

            var appUser = new ApplicationUser { Id = "user-42", UserName = "u42" };
            var officer = new Officer { Id = 300, BadgeNumber = "U42", DistrictId = 1, ApplicationUser = appUser, ApplicationUserId = appUser.Id };
            ctx.Officers.Add(officer);
            ctx.SaveChanges();

            var repo = new OfficerRepository(ctx, TestHelpers.CreateMockUserManager(), NullLogger<OfficerRepository>.Instance);

            var result = await repo.GetOfficerByUserIdAsync("user-42");

            Assert.NotNull(result);
            Assert.Equal(300, result.Id);
            Assert.Equal("U42", result.BadgeNumber);
        }

        [Fact]
        public async Task GetOfficerByUserIdAsync_Throws_WhenNotFound()
        {
            using var ctx = TestHelpers.CreateContext(Guid.NewGuid().ToString());
            var repo = new OfficerRepository(ctx, TestHelpers.CreateMockUserManager(), NullLogger<OfficerRepository>.Instance);

            await Assert.ThrowsAsync<Exception>(() => repo.GetOfficerByUserIdAsync("no-such-user"));
        }
    }
}