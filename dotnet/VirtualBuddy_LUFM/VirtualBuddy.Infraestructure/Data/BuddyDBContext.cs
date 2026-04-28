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
        }
    }
}
