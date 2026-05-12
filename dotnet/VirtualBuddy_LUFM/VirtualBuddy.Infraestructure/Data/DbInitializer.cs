using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VirtualBuddy.Domain.Project;
using VirtualBuddy.Domain.Project.Entities;
using VirtualBuddy.Domain.Project.ValueObjects;
using VirtualBuddy.Infraestructure.data;
using VirtualBuddy.Infraestructure.Identity;

namespace VirtualBuddy.Infraestructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(BuddyDBContext context, UserManager<ApplicationUser> userManager)
        {
            context.Database.EnsureCreated();

            if (await context.Technologies.AnyAsync()) return;

            // 1. Seed Technologies
            var technologies = new List<Technology>
            {
                new Technology(".NET 10"),
                new Technology("React"),
                new Technology("Angular"),
                new Technology("PostgreSQL"),
                new Technology("EF Core"),
                new Technology("Docker"),
                new Technology("Azure"),
                new Technology("Python"),
                new Technology("FastAPI"),
                new Technology("Tailwind CSS"),
                new Technology("OpenAI / RAG")
            };

            await context.Technologies.AddRangeAsync(technologies);
            await context.SaveChangesAsync();

            // 2. Seed User
            var adminUser = new ApplicationUser
            {
                UserName = "admin@virtualbuddy.com",
                Email = "admin@virtualbuddy.com",
                FullName = "Admin System",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                await userManager.CreateAsync(adminUser, "P@ssword123!");
            }
            else
            {
                adminUser = await userManager.FindByEmailAsync(adminUser.Email);
            }

            // 3. Seed Projects
            if (!await context.Projects.AnyAsync())
            {
                var projects = new List<Project>
                {
                    new Project(
                        new ProjectName("VirtualBuddy"),
                        new ProjectDescription("Mentoría virtual inteligente para equipos de desarrollo."),
                        "https://placehold.co/600x400?text=VirtualBuddy",
                        "VB",
                        DateTime.UtcNow.AddMonths(-2)
                    ),
                    new Project(
                        new ProjectName("Project Phoenix"),
                        new ProjectDescription("Migración de sistemas legados a arquitectura de microservicios."),
                        "https://placehold.co/600x400?text=Phoenix",
                        "PHX",
                        DateTime.UtcNow.AddMonths(-6)
                    ),
                    new Project(
                        new ProjectName("GreenEnergy"),
                        new ProjectDescription("Plataforma de monitoreo y optimización de paneles solares."),
                        "https://placehold.co/600x400?text=GreenEnergy",
                        "GE",
                        DateTime.UtcNow.AddMonths(-1)
                    )
                };

                // Add Relationships to first project
                projects[0].AddTechnology(technologies.First(t => t.Name == ".NET 10"));
                projects[0].AddTechnology(technologies.First(t => t.Name == "React"));
                projects[0].AddTechnology(technologies.First(t => t.Name == "OpenAI / RAG"));
                projects[0].AddMember(new ProjectMember(Guid.Parse(adminUser.Id), adminUser.FullName, "Lead Architect"));
                projects[0].SetArchitectureInfo("Clean Architecture with DDD and RAG integration for contextual AI assistance.");

                // Relationships for second project
                projects[1].AddTechnology(technologies.First(t => t.Name == "Docker"));
                projects[1].AddTechnology(technologies.First(t => t.Name == "Azure"));
                projects[1].AddTechnology(technologies.First(t => t.Name == "EF Core"));
                projects[1].AddMember(new ProjectMember(Guid.Parse(adminUser.Id), adminUser.FullName, "Senior Developer"));

                await context.Projects.AddRangeAsync(projects);
                await context.SaveChangesAsync();
            }
        }
    }
}
