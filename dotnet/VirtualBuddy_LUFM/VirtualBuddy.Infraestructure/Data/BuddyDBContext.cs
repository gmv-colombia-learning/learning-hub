using Microsoft.EntityFrameworkCore;
using VirtualBuddy.Domain.Entities;

namespace VirtualBuddy.Infraestructure.data
{
    public class BuddyDBContext : DbContext
    {

        public DbSet<Project> Projects { get; set; }

        public BuddyDBContext(DbContextOptions<BuddyDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
