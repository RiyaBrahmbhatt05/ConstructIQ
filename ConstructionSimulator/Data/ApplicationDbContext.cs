using ConstructionSimulator.Models;
using Microsoft.EntityFrameworkCore;

namespace ConstructionSimulator.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // User Management
        public DbSet<ApplicationUser> Users { get; set; }

        // Existing Sets
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<Crew> Crews { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Permit> Permits { get; set; }
        public DbSet<SimulationLog> SimulationLogs { get; set; }

        public DbSet<TaskMaterial> TaskMaterials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure Email is unique
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}