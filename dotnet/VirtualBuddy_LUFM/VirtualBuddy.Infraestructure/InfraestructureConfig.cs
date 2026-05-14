using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using System.Text;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Infraestructure.data;
using VirtualBuddy.Infraestructure.Identity;
using VirtualBuddy.Infraestructure.Persistence;
using VirtualBuddy.Infraestructure.Services;
using VirtualBuddy.Infraestructure.Util;

namespace VirtualBuddy.Infraestructure
{
    public static class InfraestructureConfig
    {
        public static IServiceCollection AddInfraConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BuddyDBContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("VirtualBuddy.Infraestructure")
                ));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<BuddyDBContext>()
            .AddDefaultTokenProviders();

            var jwtSettings = configuration.GetSection("JwtSettings");
            var secret = jwtSettings.GetValue<string>("Secret");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Ajustar a true en producción si es necesario
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
                    ValidAudience = jwtSettings.GetValue<string>("Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
                    ClockSkew = TimeSpan.Zero // Eliminar el margen de 5 minutos por defecto
                };
            });

            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IAuthService, IdentityAuthService>();

            // Configuración de Supabase
            services.Configure<SupabaseSettings>(configuration.GetSection("Supabase"));
            services.AddSingleton<IFileStorageService, SupabaseStorageService>();

            // AI Infrastructure
            services.AddScoped<IDocumentParser, DocumentParserService>();
            services.AddScoped<IKnowledgeBaseService, PostgresKnowledgeBaseService>();
            services.AddScoped<IAIService, SemanticKernelAIService>();


            // Semantic Kernel configuration with Ollama

            services.AddHttpClient("ollama", client =>
            {
                var ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
                client.BaseAddress = new Uri(ollamaEndpoint);
                client.Timeout = TimeSpan.FromMinutes(10);
            });

            services.AddScoped(sp =>
            {

                var builder = Kernel.CreateBuilder();

                var ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";

                builder.AddOllamaChatCompletion(
                    modelId: configuration["Ollama:ChatModel"] ?? "llama3",
                    endpoint: new Uri(ollamaEndpoint)
                );

                builder.AddOllamaEmbeddingGenerator(
                    modelId: configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text",
                    endpoint: new Uri(ollamaEndpoint)
                );

                return builder.Build();
            });

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            //add services dependency
            return services;
        }
    }
}
