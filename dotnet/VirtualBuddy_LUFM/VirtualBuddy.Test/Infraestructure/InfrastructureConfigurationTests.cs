using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Infraestructure;
using VirtualBuddy.Infraestructure.data;
using VirtualBuddy.Infraestructure.Services;
using VirtualBuddy.Infraestructure.Util;
using Xunit;

namespace VirtualBuddy.Test.Infraestructure
{
    public class InfrastructureConfigurationTests
    {
        [Fact]
        public void AddInfraConfigureServices_ShouldUsePostgresForLocal()
        {
            const string connectionString =
                "Host=localhost;Port=5432;Database=VirtualBuddyDB;Username=local;Password=local";
            var configuration = CreateConfiguration("Local", connectionString);
            var services = new ServiceCollection();

            services.AddInfraConfigureServices(configuration, "Local");

            using var provider = services.BuildServiceProvider();
            using var context = provider.GetRequiredService<BuddyDBContext>();
            context.Database.GetConnectionString().Should().Be(connectionString);
            context.Database.IsNpgsql().Should().BeTrue();
            context.Database.GetMigrations().Should().HaveCount(6);
            provider.GetRequiredService<IKnowledgeBaseService>()
                .Should().BeOfType<PostgresKnowledgeBaseService>();
        }

        [Fact]
        public void AddInfraConfigureServices_ShouldUseSqlServerForDevelopment()
        {
            const string connectionString =
                "Server=tcp:example.database.windows.net,1433;Initial Catalog=VirtualBuddy;User ID=dev;Password=Test.123;Encrypt=True;TrustServerCertificate=False";
            var configuration = CreateConfiguration("Development", connectionString);
            var services = new ServiceCollection();

            services.AddInfraConfigureServices(configuration, "Development");

            using var provider = services.BuildServiceProvider();
            using var context = provider.GetRequiredService<BuddyDBContext>();
            context.Database.IsSqlServer().Should().BeTrue();
            context.Database.GetMigrations().Should().ContainSingle()
                .Which.Should().EndWith("InitialAzureSql");
            var sqlConnection = context.Database.GetDbConnection().Should().BeOfType<SqlConnection>().Subject;
            sqlConnection.DataSource.Should().Be("tcp:example.database.windows.net,1433");
            sqlConnection.Database.Should().Be("VirtualBuddy");
            provider.GetRequiredService<IKnowledgeBaseService>()
                .Should().BeOfType<SqlServerKnowledgeBaseService>();
        }

        [Fact]
        public void AddInfraConfigureServices_ShouldFailWhenConnectionStringIsMissing()
        {
            var configuration = CreateConfiguration(null, null);
            var services = new ServiceCollection();

            var action = () => services.AddInfraConfigureServices(configuration, "Development");

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*ConnectionStrings:Development*");
        }

        [Fact]
        public void AddInfraConfigureServices_ShouldRejectUnsupportedEnvironment()
        {
            var configuration = CreateConfiguration(null, null);
            var services = new ServiceCollection();

            var action = () => services.AddInfraConfigureServices(configuration, "Production");

            action.Should().Throw<InvalidOperationException>().WithMessage("*no esta soportado*");
        }

        [Fact]
        public void AddInfraConfigureServices_ShouldRejectWrongProviderConnectionWithoutExposingIt()
        {
            const string connectionString = "Host=secret-host;Database=secret-database;Username=secret-user";
            var configuration = CreateConfiguration("Development", connectionString);
            var services = new ServiceCollection();

            var action = () => services.AddInfraConfigureServices(configuration, "Development");

            var exception = action.Should().Throw<InvalidOperationException>().Which;
            exception.Message.Should().Contain("ConnectionStrings:Development");
            exception.Message.Should().NotContain("secret-host");
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1999")]
        public void EmbeddingSettings_ShouldRejectUnsupportedDimensions(string dimension)
        {
            var configuration = CreateConfiguration(
                "Local",
                "Host=localhost;Database=test;Username=test;Password=test",
                dimension);
            var services = new ServiceCollection();
            services.AddInfraConfigureServices(configuration, "Local");

            using var provider = services.BuildServiceProvider();
            var action = () => provider.GetRequiredService<IOptions<EmbeddingSettings>>().Value;

            action.Should().Throw<OptionsValidationException>();
        }

        private static IConfiguration CreateConfiguration(
            string? environmentName,
            string? connectionString,
            string embeddingDimension = "768")
        {
            var values = new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "A_test_secret_that_is_at_least_32_characters_long",
                ["Supabase:Url"] = "https://example.supabase.co",
                ["Supabase:Key"] = "test-key",
                ["Supabase:BucketName"] = "documents",
                ["Supabase:ProjectImagesBucketName"] = "images",
                ["Ollama:EmbeddingDimension"] = embeddingDimension
            };

            if (environmentName is not null && connectionString is not null)
                values[$"ConnectionStrings:{environmentName}"] = connectionString;

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}
