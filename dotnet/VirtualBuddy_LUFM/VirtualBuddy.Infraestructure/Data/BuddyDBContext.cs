using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirtualBuddy.Domain.Project;
using VirtualBuddy.Domain.Project.Entities;
using VirtualBuddy.Infraestructure.Identity;

namespace VirtualBuddy.Infraestructure.data
{
    public class BuddyDBContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Technology> Technologies { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }

        public BuddyDBContext(DbContextOptions<BuddyDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(p => p.Name)
                    .HasConversion(v => v.Value, v => new Domain.Project.ValueObjects.ProjectName(v))
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.Description)
                    .HasConversion(v => v.Value, v => new Domain.Project.ValueObjects.ProjectDescription(v))
                    .IsRequired();

                // Configuración Many-to-Many con Technology
                entity.HasMany(p => p.Technologies)
                    .WithMany(t => t.Projects)
                    .UsingEntity(j => j.ToTable("ProjectTechnologies"));

                // Configuración One-to-Many con ProjectMember
                entity.HasMany(p => p.Members)
                    .WithOne()
                    .HasForeignKey(m => m.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Technology>(entity =>
            {
                entity.Property(t => t.Name).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<ProjectMember>(entity =>
            {
                entity.Property(m => m.Role).HasMaxLength(50).IsRequired();
                entity.Property(m => m.FullName).HasMaxLength(100).IsRequired();
            });
        }

        internal async Task<object> FindAsync(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }
    }
}
