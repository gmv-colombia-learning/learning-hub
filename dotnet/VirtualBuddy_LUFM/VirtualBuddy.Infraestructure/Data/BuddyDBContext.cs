using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirtualBuddy.Domain.Project;
using VirtualBuddy.Domain.Project.Entities;
using VirtualBuddy.Domain.Document;
using VirtualBuddy.Infraestructure.Identity;
using VirtualBuddy.Domain.Auth;

namespace VirtualBuddy.Infraestructure.data
{
    public class BuddyDBContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Technology> Technologies { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<PasswordRecoveryChallenge> PasswordRecoveryChallenges { get; set; }
        public DbSet<PasswordRecoveryRequest> PasswordRecoveryRequests { get; set; }

        public BuddyDBContext(DbContextOptions<BuddyDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(user => user.SessionVersion).HasDefaultValue(0);
            });

            modelBuilder.Entity<PasswordRecoveryChallenge>(entity =>
            {
                entity.ToTable("PasswordRecoveryChallenges");
                entity.HasKey(challenge => challenge.Id);
                entity.Property(challenge => challenge.UserId).IsRequired().HasMaxLength(450);
                entity.Property(challenge => challenge.CodeHash).IsRequired().HasMaxLength(64);
                entity.Property(challenge => challenge.ConcurrencyStamp).IsConcurrencyToken();
                entity.HasIndex(challenge => new { challenge.UserId, challenge.IssuedAt });
            });

            modelBuilder.Entity<PasswordRecoveryRequest>(entity =>
            {
                entity.ToTable("PasswordRecoveryRequests");
                entity.HasKey(request => request.Id);
                entity.Property(request => request.EmailHash).IsRequired().HasMaxLength(64);
                entity.Property(request => request.OriginHash).IsRequired().HasMaxLength(64);
                entity.HasIndex(request => new { request.EmailHash, request.RequestedAt });
                entity.HasIndex(request => new { request.OriginHash, request.RequestedAt });
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Documents");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.PublicUrl).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.Size).HasMaxLength(50);
                
                entity.HasIndex(e => e.ProjectId);
            });

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
