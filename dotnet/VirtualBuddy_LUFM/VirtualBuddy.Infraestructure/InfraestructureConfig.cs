using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Npgsql;
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
        private const string AzureOpenAIDevelopmentHttpClient = "AzureOpenAIDevelopment";

        public static IServiceCollection AddInfraConfigureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            string environmentName)
        {
            if (environmentName is not ("Local" or "Development"))
                throw new InvalidOperationException(
                    $"El ambiente '{environmentName}' no esta soportado. Use 'Local' o 'Development'.");

            var connectionString = configuration.GetConnectionString(environmentName);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    $"La configuracion 'ConnectionStrings:{environmentName}' es obligatoria.");

            if (environmentName == "Local")
            {
                ValidatePostgresConnectionString(connectionString, environmentName);
                services.AddDbContext<BuddyDBContext>(options =>
                    options.UseNpgsql(
                        connectionString,
                        database => database.MigrationsAssembly("VirtualBuddy.Infraestructure")));
                services.AddScoped<IKnowledgeBaseService, PostgresKnowledgeBaseService>();
            }
            else
            {
                ValidateSqlServerConnectionString(connectionString, environmentName);
                services.AddDbContext<BuddyDBContext>(options =>
                    options.UseSqlServer(
                        connectionString,
                        database => database.MigrationsAssembly("VirtualBuddy.Migrations.SqlServer")));
                services.AddScoped<IKnowledgeBaseService, SqlServerKnowledgeBaseService>();
            }

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
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (context.Principal == null)
                        {
                            context.Fail("El token no identifica al usuario.");
                            return;
                        }

                        var validator = context.HttpContext.RequestServices
                            .GetRequiredService<JwtSessionValidator>();
                        if (!await validator.IsValidAsync(context.Principal))
                            context.Fail("La sesion fue invalidada.");
                    }
                };
            });

            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IAuthService, IdentityAuthService>();
            services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
            services.AddHttpClient<IEmailSender, ResendEmailSender>(client =>
            {
                client.BaseAddress = new Uri("https://api.resend.com/");
            });
            services.AddScoped<JwtSessionValidator>();
            services.AddSingleton(TimeProvider.System);
            var embeddingSectionName = environmentName == "Local"
                ? EmbeddingSettings.SectionName
                : AzureOpenAISettings.SectionName;

            services.AddOptions<EmbeddingSettings>()
                .Bind(configuration.GetSection(embeddingSectionName))
                .Validate(
                    settings => settings.EmbeddingDimension is > 0 and <= EmbeddingSettings.MaximumAzureSqlDimensions,
                    $"{embeddingSectionName}:EmbeddingDimension debe estar entre 1 y {EmbeddingSettings.MaximumAzureSqlDimensions}.")
                .ValidateOnStart();

            if (environmentName == "Development")
            {
                services.AddOptions<AzureOpenAISettings>()
                    .Bind(configuration.GetSection(AzureOpenAISettings.SectionName))
                    .Validate(settings =>
                            Uri.TryCreate(settings.Endpoint, UriKind.Absolute, out var endpoint) &&
                            endpoint.Scheme == Uri.UriSchemeHttps &&
                            !string.IsNullOrWhiteSpace(settings.ApiKey) &&
                            !string.IsNullOrWhiteSpace(settings.ChatDeploymentName) &&
                            !string.IsNullOrWhiteSpace(settings.EmbeddingDeploymentName) &&
                            settings.EmbeddingDimension == 768,
                        "La configuracion de AzureOpenAI es incompleta o invalida.")
                    .ValidateOnStart();

                services.AddLogging();
                services.AddSingleton<AzureOpenAIDevelopmentCertificateValidator>();
                services.AddHttpClient(AzureOpenAIDevelopmentHttpClient, client =>
                    {
                        client.Timeout = TimeSpan.FromMinutes(10);
                    })
                    .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                    {
                        var validator = serviceProvider
                            .GetRequiredService<AzureOpenAIDevelopmentCertificateValidator>();

                        return new HttpClientHandler
                        {
                            AllowAutoRedirect = false,
                            ServerCertificateCustomValidationCallback = validator.Validate
                        };
                    })
                    .AddAsKeyed(ServiceLifetime.Scoped);
            }
            services.AddOptions<PasswordRecoverySettings>()
                .Bind(configuration.GetSection(PasswordRecoverySettings.SectionName))
                .Validate(settings => settings.CodePepper?.Length >= 32,
                    "PasswordRecovery:CodePepper debe contener al menos 32 caracteres.")
                .ValidateOnStart();
            services.AddOptions<ResendSettings>()
                .Bind(configuration.GetSection(ResendSettings.SectionName))
                .Validate(settings =>
                        !string.IsNullOrWhiteSpace(settings.ApiKey) &&
                        !string.IsNullOrWhiteSpace(settings.SenderEmail) &&
                        !string.IsNullOrWhiteSpace(settings.SenderName),
                    "La configuracion de Resend es incompleta.")
                .ValidateOnStart();

            // Configuración de Supabase
            services.AddOptions<SupabaseSettings>()
                .Bind(configuration.GetSection(SupabaseSettings.SectionName))
                .Validate(settings =>
                        Uri.TryCreate(settings.Url, UriKind.Absolute, out _) &&
                        !string.IsNullOrWhiteSpace(settings.Key) &&
                        !string.IsNullOrWhiteSpace(settings.BucketName) &&
                        !string.IsNullOrWhiteSpace(settings.ProjectImagesBucketName),
                    "La configuracion de Supabase es incompleta.")
                .ValidateOnStart();
            services.AddSingleton<SupabaseStorageService>();
            services.AddSingleton<IFileStorageService>(provider => provider.GetRequiredService<SupabaseStorageService>());
            services.AddSingleton<IProjectImageStorageService>(provider => provider.GetRequiredService<SupabaseStorageService>());

            // AI Infrastructure
            services.AddScoped<IDocumentParser, DocumentParserService>();
            services.AddScoped<IAIService, SemanticKernelAIService>();


            if (environmentName == "Local")
            {
                services.AddHttpClient("ollama", client =>
                {
                    var ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
                    client.BaseAddress = new Uri(ollamaEndpoint);
                    client.Timeout = TimeSpan.FromMinutes(10);
                });
            }

            services.AddScoped(sp =>
            {
                var builder = Kernel.CreateBuilder();

                if (environmentName == "Local")
                {
                    var ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";

                    builder.AddOllamaChatCompletion(
                        modelId: configuration["Ollama:ChatModel"] ?? "llama3",
                        endpoint: new Uri(ollamaEndpoint));

                    builder.AddOllamaEmbeddingGenerator(
                        modelId: configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text",
                        endpoint: new Uri(ollamaEndpoint));
                }
                else
                {
                    var settings = sp.GetRequiredService<IOptions<AzureOpenAISettings>>().Value;
                    var httpClient = sp.GetRequiredKeyedService<HttpClient>(
                        AzureOpenAIDevelopmentHttpClient);

                    builder.AddAzureOpenAIChatCompletion(
                        deploymentName: settings.ChatDeploymentName,
                        endpoint: settings.Endpoint,
                        apiKey: settings.ApiKey,
                        httpClient: httpClient);

#pragma warning disable SKEXP0010
                    builder.AddAzureOpenAIEmbeddingGenerator(
                        deploymentName: settings.EmbeddingDeploymentName,
                        endpoint: settings.Endpoint,
                        apiKey: settings.ApiKey,
                        dimensions: settings.EmbeddingDimension,
                        httpClient: httpClient);
#pragma warning restore SKEXP0010
                }

                return builder.Build();
            });

            return services;
        }

        private static void ValidatePostgresConnectionString(string connectionString, string environmentName)
        {
            try
            {
                _ = new NpgsqlConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException(
                    $"La configuracion 'ConnectionStrings:{environmentName}' no es una cadena PostgreSQL valida.");
            }
        }

        private static void ValidateSqlServerConnectionString(string connectionString, string environmentName)
        {
            try
            {
                _ = new SqlConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException(
                    $"La configuracion 'ConnectionStrings:{environmentName}' no es una cadena SQL Server valida.");
            }
        }

        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            //add services dependency
            return services;
        }
    }
}
