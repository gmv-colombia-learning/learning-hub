using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtualBuddy.Infraestructure;
using VirtualBuddy.Infraestructure.data;
using Xunit;

namespace VirtualBuddy.Test.Infraestructure
{
    public class InfrastructureConfigurationTests
    {
        [Theory]
        [InlineData("Local", "Host=localhost;Port=5432;Database=VirtualBuddyDB;Username=local;Password=local")]
        [InlineData("Development", "Host=db.example.supabase.co;Port=5432;Database=postgres;Username=dev;Password=dev;SSL Mode=Require")]
        public void AddInfraConfigureServices_ShouldUseEnvironmentPostgresConnection(
            string environmentName,
            string connectionString)
        {
            var configuration = CreateConfiguration(environmentName, connectionString);
            var services = new ServiceCollection();

            services.AddInfraConfigureServices(configuration, environmentName);

            using var provider = services.BuildServiceProvider();
            using var context = provider.GetRequiredService<BuddyDBContext>();
            context.Database.GetConnectionString().Should().Be(connectionString);
        }

        [Fact]
        public void AddInfraConfigureServices_ShouldFailWhenConnectionStringIsMissing()
        {
            var configuration = CreateConfiguration(null, null);
            var services = new ServiceCollection();

            var action = () => services.AddInfraConfigureServices(configuration, "Local");

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*ConnectionStrings:DefaultConnection*");
        }

        [Fact]
        public void AddInfraConfigureServices_ShouldUseDefaultConnectionAsFallback()
        {
            const string connectionString =
                "Host=localhost;Port=5432;Database=VirtualBuddyDB;Username=fallback;Password=fallback";
            var values = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["JwtSettings:Secret"] = "A_test_secret_that_is_at_least_32_characters_long"
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
            var services = new ServiceCollection();

            services.AddInfraConfigureServices(configuration, "Local");

            using var provider = services.BuildServiceProvider();
            using var context = provider.GetRequiredService<BuddyDBContext>();
            context.Database.GetConnectionString().Should().Be(connectionString);
        }

        private static IConfiguration CreateConfiguration(string? environmentName, string? connectionString)
        {
            var values = new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "A_test_secret_that_is_at_least_32_characters_long",
                ["Supabase:Url"] = "https://example.supabase.co",
                ["Supabase:Key"] = "test-key",
                ["Supabase:BucketName"] = "documents",
                ["Supabase:ProjectImagesBucketName"] = "images"
            };

            if (environmentName is not null && connectionString is not null)
                values[$"ConnectionStrings:{environmentName}"] = connectionString;

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}
