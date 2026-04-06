using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using PotholeApi.Models;
using static PotholeApi.Models.Enums;

namespace PotholeApi
{
    public class Context: IdentityDbContext<ApplicationUser>
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
            //
        }
        public DbSet<Pothole> Potholes { get; set; }
        public DbSet<Officer> Officers { get; set; }
        public DbSet<JobTask> JobTasks { get; set; }
        public DbSet<RepairTeam> RepairTeams { get; set; }
        public DbSet<Street> Streets { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Neighbourhood> Neighbourhoods { get; set; }
        public DbSet<ScanExecutionTask> ScanExecutionTasks { get; set; }
        public DbSet<RoutePoint> RoutePoints { get; set; }
        public DbSet<GpsCoordinate> GpsCoordinates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Link Officer (int PK) to ApplicationUser (string Id) as a 1:1 optional relationship
            modelBuilder.Entity<Officer>()
                        .HasOne(o => o.ApplicationUser)
                        .WithOne()
                        .HasForeignKey<Officer>(o => o.ApplicationUserId)
                        .IsRequired(false)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<District>()
                .Property(d => d.Geometry)
                .HasColumnType("geography");   // Important

            modelBuilder.Entity<Pothole>(entity =>
            {
                // 1. Ensure it is Geography (which you already did)
                entity.Property(p => p.Location)
                      .HasColumnType("geography");

            });
            modelBuilder.Entity<Neighbourhood>()
            .Property(n => n.Geometry)
            .HasColumnType("geography"); // <--- Keeps everything consistent
            modelBuilder.Entity<Models.JobTask>(entity =>
            {
                entity.HasKey(t => t.Id);

                // Officer (one -> many)
                entity
                    .HasOne(t => t.Officer)
                    .WithMany(o => o.JobTasks)
                    .HasForeignKey(t => t.OfficerId) 
                    .OnDelete(DeleteBehavior.Restrict);

                // RepairTeam (one -> many)
                entity
                    .HasOne(t => t.RepairTeam)
                    .WithMany(rt => rt.Tasks)
                    .HasForeignKey(t => t.RepairTeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Pothole (one-to-one): a Pothole can be associated with at most one Task.
                entity
                    .HasOne(t => t.Pothole)
                    .WithOne()
                    .HasForeignKey<Models.JobTask>(t => t.PotholeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add the Priority configuration here
                entity.Property(t => t.Priority)
                      .HasDefaultValue(Priority.Low);  // DB default for new rows
                                      // make column non-nullable
            });

        }

    }
}
