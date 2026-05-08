using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirtualBuddy.Domain.Project;
using VirtualBuddy.Infraestructure.Identity;

namespace VirtualBuddy.Infraestructure.data
{
    public class BuddyDBContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<Project> Projects { get; set; }

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
            });
        }

        internal async Task<object> FindAsync(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }
    }
}
