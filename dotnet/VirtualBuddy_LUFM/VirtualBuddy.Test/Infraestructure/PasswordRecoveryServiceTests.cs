using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Domain.Common.Exceptions;
using VirtualBuddy.Infraestructure.data;
using VirtualBuddy.Infraestructure.Identity;
using VirtualBuddy.Infraestructure.Util;
using Xunit;

namespace VirtualBuddy.Test.Infraestructure
{
    public class PasswordRecoveryServiceTests
    {
        [Fact]
        public async Task RequestAndReset_ShouldChangePasswordConsumeCodeAndRevokeOldSession()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();

            await context.RequestCodeAsync(user.Email!);
            var code = context.EmailSender.LastRecoveryCode!;

            await context.ResetPasswordAsync(user.Email!, code.ToLowerInvariant(), "NewPassword2");

            await using var scope = context.Services.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var updatedUser = await userManager.FindByEmailAsync(user.Email!);
            updatedUser.Should().NotBeNull();
            (await userManager.CheckPasswordAsync(updatedUser!, "NewPassword2")).Should().BeTrue();
            (await userManager.CheckPasswordAsync(updatedUser!, TestContext.InitialPassword)).Should().BeFalse();
            updatedUser!.SessionVersion.Should().Be(1);
            context.EmailSender.PasswordChangedMessages.Should().Be(1);

            var validator = scope.ServiceProvider.GetRequiredService<JwtSessionValidator>();
            (await validator.IsValidAsync(CreatePrincipal(user.Id, 0))).Should().BeFalse();
            (await validator.IsValidAsync(CreatePrincipal(user.Id, 1))).Should().BeTrue();

            var database = scope.ServiceProvider.GetRequiredService<BuddyDBContext>();
            var challenge = await database.PasswordRecoveryChallenges.SingleAsync();
            challenge.ConsumedAt.Should().NotBeNull();
            challenge.CodeHash.Should().NotContain(code);
        }

        [Fact]
        public async Task Request_WhenAccountDoesNotExist_ShouldNotSendEmail()
        {
            await using var context = await TestContext.CreateAsync();

            await context.RequestCodeAsync("unknown@example.com");

            context.EmailSender.RecoveryMessages.Should().Be(0);
            await using var scope = context.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<BuddyDBContext>();
            (await database.PasswordRecoveryChallenges.CountAsync()).Should().Be(0);
            (await database.PasswordRecoveryRequests.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Request_WhenRepeatedWithinMinute_ShouldBeRateLimited()
        {
            await using var context = await TestContext.CreateAsync();

            await context.RequestCodeAsync("unknown@example.com");
            var act = () => context.RequestCodeAsync("unknown@example.com", "10.0.0.2");

            await act.Should().ThrowAsync<TooManyRequestsException>();
        }

        [Fact]
        public async Task Request_WhenOriginRepeatsWithinMinute_ShouldBeRateLimitedAcrossEmails()
        {
            await using var context = await TestContext.CreateAsync();

            await context.RequestCodeAsync("first@example.com", "10.0.0.1");
            var act = () => context.RequestCodeAsync("second@example.com", "10.0.0.1");

            await act.Should().ThrowAsync<TooManyRequestsException>();
        }

        [Fact]
        public async Task Request_WhenSixthRequestOccursWithinHour_ShouldBeRateLimited()
        {
            await using var context = await TestContext.CreateAsync();

            for (var request = 0; request < 5; request++)
            {
                await context.RequestCodeAsync("unknown@example.com");
                context.Time.Advance(TimeSpan.FromMinutes(1));
            }

            var act = () => context.RequestCodeAsync("unknown@example.com");

            await act.Should().ThrowAsync<TooManyRequestsException>();
        }

        [Fact]
        public async Task Request_WhenNewCodeIsSent_ShouldInvalidatePreviousCode()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();
            await context.RequestCodeAsync(user.Email!);
            var previousCode = context.EmailSender.LastRecoveryCode!;
            context.Time.Advance(TimeSpan.FromMinutes(1));

            await context.RequestCodeAsync(user.Email!, "10.0.0.2");
            var currentCode = context.EmailSender.LastRecoveryCode!;

            var previousAttempt = () => context.ResetPasswordAsync(
                user.Email!,
                previousCode,
                "NewPassword2");
            await previousAttempt.Should().ThrowAsync<ValidationException>();

            await context.ResetPasswordAsync(user.Email!, currentCode, "NewPassword2");
        }

        [Fact]
        public async Task Request_WhenEmailProviderFails_ShouldInvalidatePreviousAndNewCodes()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();
            await context.RequestCodeAsync(user.Email!);
            context.Time.Advance(TimeSpan.FromMinutes(1));
            context.EmailSender.FailRecovery = true;

            var act = () => context.RequestCodeAsync(user.Email!, "10.0.0.2");

            await act.Should().ThrowAsync<TemporaryServiceUnavailableException>();
            await using var scope = context.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<BuddyDBContext>();
            var challenges = await database.PasswordRecoveryChallenges.ToListAsync();
            challenges.Should().HaveCount(2);
            challenges.Should().OnlyContain(challenge => challenge.InvalidatedAt.HasValue);
        }

        [Fact]
        public async Task Reset_AfterThreeWrongCodes_ShouldInvalidateChallenge()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();
            await context.RequestCodeAsync(user.Email!);

            for (var attempt = 0; attempt < 3; attempt++)
            {
                var act = () => context.ResetPasswordAsync(user.Email!, "ZZZ999", "NewPassword2");
                await act.Should().ThrowAsync<ValidationException>();
            }

            var correctAttempt = () => context.ResetPasswordAsync(
                user.Email!,
                context.EmailSender.LastRecoveryCode!,
                "NewPassword2");
            await correctAttempt.Should().ThrowAsync<ValidationException>();

            await using var scope = context.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<BuddyDBContext>();
            var challenge = await database.PasswordRecoveryChallenges.SingleAsync();
            challenge.FailedAttempts.Should().Be(3);
            challenge.InvalidatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task Reset_WhenCodeExpiredAfterContextRestart_ShouldFail()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();
            await context.RequestCodeAsync(user.Email!);
            var code = context.EmailSender.LastRecoveryCode!;
            context.Time.Advance(TimeSpan.FromMinutes(15));

            var act = () => context.ResetPasswordAsync(user.Email!, code, "NewPassword2");

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Reset_WhenNewPasswordMatchesCurrent_ShouldNotConsumeCode()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();
            await context.RequestCodeAsync(user.Email!);

            var act = () => context.ResetPasswordAsync(
                user.Email!,
                context.EmailSender.LastRecoveryCode!,
                TestContext.InitialPassword);

            await act.Should().ThrowAsync<ValidationException>();
            await using var scope = context.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<BuddyDBContext>();
            (await database.PasswordRecoveryChallenges.SingleAsync()).ConsumedAt.Should().BeNull();
        }

        [Fact]
        public async Task Reset_WhenPasswordViolatesPolicy_ShouldNotConsumeCode()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();
            await context.RequestCodeAsync(user.Email!);

            var act = () => context.ResetPasswordAsync(
                user.Email!,
                context.EmailSender.LastRecoveryCode!,
                "weak");

            await act.Should().ThrowAsync<ValidationException>();
            await using var scope = context.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<BuddyDBContext>();
            (await database.PasswordRecoveryChallenges.SingleAsync()).ConsumedAt.Should().BeNull();
        }

        [Fact]
        public async Task Reset_WhenCodeIsReused_ShouldFail()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();
            await context.RequestCodeAsync(user.Email!);
            var code = context.EmailSender.LastRecoveryCode!;
            await context.ResetPasswordAsync(user.Email!, code, "NewPassword2");

            var act = () => context.ResetPasswordAsync(user.Email!, code, "AnotherPassword3");

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Reset_WhenSubmittedConcurrently_ShouldCompleteAtMostOnce()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();
            await context.RequestCodeAsync(user.Email!);
            var code = context.EmailSender.LastRecoveryCode!;

            var attempts = await Task.WhenAll(
                TryResetAsync(context, user.Email!, code, "NewPassword2"),
                TryResetAsync(context, user.Email!, code, "AnotherPassword3"));

            attempts.Count(succeeded => succeeded).Should().Be(1);
        }

        [Fact]
        public async Task Reset_WhenNotificationFails_ShouldKeepPasswordChanged()
        {
            await using var context = await TestContext.CreateAsync();
            var user = await context.CreateUserAsync();
            await context.RequestCodeAsync(user.Email!);
            context.EmailSender.FailPasswordChanged = true;

            await context.ResetPasswordAsync(
                user.Email!,
                context.EmailSender.LastRecoveryCode!,
                "NewPassword2");

            await using var scope = context.Services.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var updatedUser = await userManager.FindByEmailAsync(user.Email!);
            (await userManager.CheckPasswordAsync(updatedUser!, "NewPassword2")).Should().BeTrue();
        }

        [Fact]
        public async Task RegisterAndLogin_ShouldContinueIssuingCurrentSessionJwt()
        {
            await using var context = await TestContext.CreateAsync();
            await using var scope = context.Services.CreateAsyncScope();
            var authService = scope.ServiceProvider.GetRequiredService<IdentityAuthService>();

            var registration = await authService.RegisterAsync(new RegisterRequestDto
            {
                Email = "new-user@example.com",
                FullName = "New User",
                Password = "ValidPassword1"
            });
            var login = await authService.LoginAsync(new LoginRequestDto
            {
                Email = "new-user@example.com",
                Password = "ValidPassword1"
            });

            registration.Token.Should().NotBeNullOrWhiteSpace();
            login.Token.Should().NotBeNullOrWhiteSpace();
            var token = new JwtSecurityTokenHandler().ReadJwtToken(login.Token);
            token.Claims.Single(claim => claim.Type == "session_version").Value.Should().Be("0");
        }

        private static ClaimsPrincipal CreatePrincipal(string userId, int sessionVersion)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId),
                new Claim("session_version", sessionVersion.ToString())
            }, "test"));
        }

        private static async Task<bool> TryResetAsync(
            TestContext context,
            string email,
            string code,
            string password)
        {
            try
            {
                await context.ResetPasswordAsync(email, code, password);
                return true;
            }
            catch (Exception exception) when (exception is DomainException or DbUpdateException)
            {
                return false;
            }
        }

        private sealed class TestContext : IAsyncDisposable
        {
            public const string InitialPassword = "OldPassword1";

            public ServiceProvider Services { get; }
            public FakeEmailSender EmailSender { get; }
            public MutableTimeProvider Time { get; }

            private TestContext(
                ServiceProvider services,
                FakeEmailSender emailSender,
                MutableTimeProvider time)
            {
                Services = services;
                EmailSender = emailSender;
                Time = time;
            }

            public static async Task<TestContext> CreateAsync()
            {
                var emailSender = new FakeEmailSender();
                var time = new MutableTimeProvider(
                    new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero));
                var databaseName = Guid.NewGuid().ToString();

                var services = new ServiceCollection();
                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Critical));
                services.AddDataProtection();
                services.AddAuthentication();
                services.AddHttpContextAccessor();
                services.AddDbContext<BuddyDBContext>(options =>
                    options
                        .UseInMemoryDatabase(databaseName)
                        .ConfigureWarnings(warnings =>
                            warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
                services.AddIdentityCore<ApplicationUser>(options =>
                    {
                        options.Password.RequireDigit = true;
                        options.Password.RequiredLength = 8;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = true;
                        options.Password.RequireLowercase = true;
                    })
                    .AddSignInManager()
                    .AddEntityFrameworkStores<BuddyDBContext>()
                    .AddDefaultTokenProviders();
                services.AddSingleton<IEmailSender>(emailSender);
                services.AddSingleton<TimeProvider>(time);
                services.Configure<PasswordRecoverySettings>(settings =>
                    settings.CodePepper = "test-only-password-recovery-pepper-32-characters");
                services.AddScoped<PasswordRecoveryService>();
                services.AddScoped<JwtSessionValidator>();
                services.AddScoped<IdentityAuthService>();
                services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["JwtSettings:Secret"] = "test-only-jwt-secret-at-least-32-characters-long",
                        ["JwtSettings:Issuer"] = "VirtualBuddy.Test",
                        ["JwtSettings:Audience"] = "VirtualBuddy.Test",
                        ["JwtSettings:ExpiryInMinutes"] = "60"
                    })
                    .Build());

                var provider = services.BuildServiceProvider();
                await using var scope = provider.CreateAsyncScope();
                var database = scope.ServiceProvider.GetRequiredService<BuddyDBContext>();
                await database.Database.EnsureCreatedAsync();

                return new TestContext(provider, emailSender, time);
            }

            public async Task<ApplicationUser> CreateUserAsync(string email = "user@example.com")
            {
                await using var scope = Services.CreateAsyncScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Test User"
                };
                var result = await userManager.CreateAsync(user, InitialPassword);
                result.Succeeded.Should().BeTrue();
                return user;
            }

            public async Task RequestCodeAsync(
                string email,
                string origin = "10.0.0.1")
            {
                await using var scope = Services.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<PasswordRecoveryService>();
                await service.RequestCodeAsync(email, origin);
            }

            public async Task ResetPasswordAsync(string email, string code, string password)
            {
                await using var scope = Services.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<PasswordRecoveryService>();
                await service.ResetPasswordAsync(email, code, password);
            }

            public async ValueTask DisposeAsync()
            {
                await Services.DisposeAsync();
            }
        }

        private sealed class FakeEmailSender : IEmailSender
        {
            public string? LastRecoveryCode { get; private set; }
            public int RecoveryMessages { get; private set; }
            public int PasswordChangedMessages { get; private set; }
            public bool FailRecovery { get; set; }
            public bool FailPasswordChanged { get; set; }

            public Task SendRecoveryCodeAsync(
                string recipient,
                string code,
                TimeSpan validity,
                CancellationToken cancellationToken = default)
            {
                if (FailRecovery)
                    throw new InvalidOperationException("Email provider unavailable");

                LastRecoveryCode = code;
                RecoveryMessages++;
                code.Should().MatchRegex("^[A-Z0-9]{6}$");
                validity.Should().Be(TimeSpan.FromMinutes(15));
                return Task.CompletedTask;
            }

            public Task SendPasswordChangedAsync(
                string recipient,
                CancellationToken cancellationToken = default)
            {
                PasswordChangedMessages++;
                if (FailPasswordChanged)
                    throw new InvalidOperationException("Email provider unavailable");

                return Task.CompletedTask;
            }
        }

        private sealed class MutableTimeProvider : TimeProvider
        {
            private DateTimeOffset _utcNow;

            public MutableTimeProvider(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }

            public override DateTimeOffset GetUtcNow() => _utcNow;

            public void Advance(TimeSpan duration)
            {
                _utcNow = _utcNow.Add(duration);
            }
        }
    }
}
