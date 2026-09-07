using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VirtualBuddy.Domain.Common.Exceptions;
using VirtualBuddy.Infraestructure.data;
using VirtualBuddy.Infraestructure.Services;
using VirtualBuddy.Infraestructure.Util;
using Xunit;

namespace VirtualBuddy.Test.Infraestructure
{
    public sealed class KnowledgeBaseDimensionTests
    {
        [Fact]
        public async Task SqlServerAddChunks_ShouldRejectInvalidDimensionBeforeDatabaseAccess()
        {
            await using var context = CreateContext();
            var service = new SqlServerKnowledgeBaseService(context, CreateSettings(3));
            var chunks = new[] { ("content", new[] { 1f, 0f }, (string?)null) };

            var action = () => service.AddChunksAsync(Guid.NewGuid(), null, chunks);

            await action.Should().ThrowAsync<ValidationException>().WithMessage("*debe ser 3*");
        }

        [Fact]
        public async Task PostgresSearch_ShouldRejectInvalidDimensionBeforeDatabaseAccess()
        {
            await using var context = CreateContext();
            var service = new PostgresKnowledgeBaseService(context, CreateSettings(3));

            var action = () => service.SearchRelevantChunksAsync(Guid.NewGuid(), [1f, 0f]);

            await action.Should().ThrowAsync<ValidationException>().WithMessage("*debe ser 3*");
        }

        [Fact]
        public async Task EmptyChunkBatch_ShouldNotAccessDatabase()
        {
            await using var context = CreateContext();
            var sqlServerService = new SqlServerKnowledgeBaseService(context, CreateSettings(3));
            var postgresService = new PostgresKnowledgeBaseService(context, CreateSettings(3));

            await sqlServerService.AddChunksAsync(Guid.NewGuid(), null, []);
            await postgresService.AddChunksAsync(Guid.NewGuid(), null, []);
        }

        private static BuddyDBContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<BuddyDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new BuddyDBContext(options);
        }

        private static IOptions<EmbeddingSettings> CreateSettings(int dimension)
        {
            return Options.Create(new EmbeddingSettings { EmbeddingDimension = dimension });
        }
    }
}
