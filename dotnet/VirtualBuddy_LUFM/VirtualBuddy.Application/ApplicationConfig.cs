using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using VirtualBuddy.Application.Project;
using VirtualBuddy.Application.Project.UseCases;
using VirtualBuddy.Application.Auth;
using VirtualBuddy.Application.Auth.UseCases;
using VirtualBuddy.Application.Document;
using VirtualBuddy.Application.Document.UseCases;

namespace VirtualBuddy.Application
{
    public static class ApplicationConfig
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            
            // Configuración para que Mapster ignore los valores nulos al mapear (esencial para PATCH)
            config.Default.IgnoreNullValues(true);

            // Configuración para mapear Value Objects a string
            config.NewConfig<VirtualBuddy.Domain.Project.ValueObjects.ProjectName, string>()
                .MapWith(src => src.Value);
            config.NewConfig<VirtualBuddy.Domain.Project.ValueObjects.ProjectDescription, string>()
                .MapWith(src => src.Value);
            
            config.Scan(Assembly.GetExecutingAssembly());

            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            services.AddScoped<GetProjects>();
            services.AddScoped<GetProjectById>();
            services.AddScoped<CreateProject>();
            services.AddScoped<UpdateProject>();
            services.AddScoped<PatchProject>();
            services.AddScoped<DeleteProject>();
            services.AddScoped<AddTechnologyToProject>();
            services.AddScoped<GetTechnologies>();
            services.AddScoped<ProjectFacade>();

            services.AddScoped<Login>();
            services.AddScoped<Register>();
            services.AddScoped<AuthFacade>();

            // Documentos
            services.AddScoped<UploadDocument>();
            services.AddScoped<GetProjectDocuments>();
            services.AddScoped<DeleteDocument>();
            services.AddScoped<DocumentFacade>();

            return services;
        }
    }
}
