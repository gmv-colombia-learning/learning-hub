using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Infraestructure.data;
using VirtualBuddy.Infraestructure.Persistence;

namespace VirtualBuddy.Infraestructure
{
    public static class InfraestructureConfig
    {
        public static IServiceCollection AddConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BuddyDBContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("VirtualBuddy.Infraestructure")
                ));

            services.AddScoped<IRepository, Repository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            //add services dependency
            return services;
        }
    }
}
