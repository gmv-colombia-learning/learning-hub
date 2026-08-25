using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Domain.Auth;
using VirtualBuddy.Domain.Common.Exceptions;
using VirtualBuddy.Infraestructure.data;
using VirtualBuddy.Infraestructure.Util;

namespace VirtualBuddy.Infraestructure.Identity
{
    public class PasswordRecoveryService : IPasswordRecoveryService
    {
        private const string CodeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const string InvalidCodeMessage = "El codigo no es valido o ha expirado.";
        private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(15);

        private readonly BuddyDBContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly PasswordRecoverySettings _settings;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<PasswordRecoveryService> _logger;

        public PasswordRecoveryService(
            BuddyDBContext dbContext,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IOptions<PasswordRecoverySettings> settings,
            TimeProvider timeProvider,
            ILogger<PasswordRecoveryService> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _emailSender = emailSender;
            _settings = settings.Value;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task RequestCodeAsync(
            string email,
            string origin,
            CancellationToken cancellationToken = default)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var normalizedEmail = _userManager.NormalizeEmail(email) ?? email.ToUpperInvariant();
            var emailHash = HashValue($"email:{normalizedEmail}");
            var originHash = HashValue($"origin:{origin}");

            await RegisterRequestAsync(emailHash, originHash, now, cancellationToken);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return;

            var code = GenerateCode();
            var challenge = PasswordRecoveryChallenge.CreatePending(
                user.Id,
                HashValue($"code:{user.Id}:{code}"),
                now);

            await ReplaceWithPendingChallengeAsync(challenge, now, cancellationToken);

            try
            {
                await _emailSender.SendRecoveryCodeAsync(email, code, CodeLifetime, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await InvalidateChallengeAsync(challenge, now, CancellationToken.None);
                throw;
            }
            catch (Exception exception)
            {
                await InvalidateChallengeAsync(challenge, now, CancellationToken.None);
                _logger.LogError(
                    "No se pudo enviar un codigo de recuperacion. Tipo: {ExceptionType}",
                    exception.GetType().Name);
                throw new TemporaryServiceUnavailableException(
                    "El servicio de recuperacion no esta disponible temporalmente.");
            }

            challenge.Activate(_timeProvider.GetUtcNow().UtcDateTime, CodeLifetime);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ResetPasswordAsync(
            string email,
            string code,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new ValidationException(InvalidCodeMessage);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            await AcquireLockAsync($"password-reset:{user.Id}", cancellationToken);

            var challenge = await _dbContext.PasswordRecoveryChallenges
                .Where(item => item.UserId == user.Id &&
                               item.InvalidatedAt == null &&
                               item.ConsumedAt == null)
                .OrderByDescending(item => item.IssuedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (challenge == null || !challenge.IsActive(now))
                throw new ValidationException(InvalidCodeMessage);

            var suppliedHash = HashValue($"code:{user.Id}:{code.ToUpperInvariant()}");
            if (!HashesMatch(challenge.CodeHash, suppliedHash))
            {
                challenge.RegisterFailedAttempt(now);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var message = challenge.FailedAttempts >= PasswordRecoveryChallenge.MaximumFailedAttempts
                    ? "El codigo no es valido o ha expirado. Solicite uno nuevo."
                    : InvalidCodeMessage;
                throw new ValidationException(message);
            }

            if (await _userManager.CheckPasswordAsync(user, newPassword))
                throw new ValidationException("La contrasena nueva debe ser diferente de la vigente.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!resetResult.Succeeded)
            {
                var errors = string.Join(", ", resetResult.Errors.Select(error => error.Description));
                throw new ValidationException(errors);
            }

            user.RevokeSessions();
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new ConflictException("No se pudo invalidar las sesiones anteriores.");

            challenge.Consume(now);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            try
            {
                await _emailSender.SendPasswordChangedAsync(email, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Se cancelo el envio del aviso de cambio de contrasena.");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "No se pudo enviar el aviso de cambio de contrasena. Tipo: {ExceptionType}",
                    exception.GetType().Name);
            }
        }

        private async Task RegisterRequestAsync(
            string emailHash,
            string originHash,
            DateTime now,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            foreach (var lockKey in new[] { $"email:{emailHash}", $"origin:{originHash}" }.Order())
                await AcquireLockAsync(lockKey, cancellationToken);

            var cutoff = now.AddHours(-1);
            var expiredRequests = _dbContext.PasswordRecoveryRequests
                .Where(request => request.RequestedAt <= cutoff &&
                                  (request.EmailHash == emailHash || request.OriginHash == originHash));
            if (_dbContext.Database.IsRelational())
                await expiredRequests.ExecuteDeleteAsync(cancellationToken);
            else
                _dbContext.PasswordRecoveryRequests.RemoveRange(
                    await expiredRequests.ToListAsync(cancellationToken));

            var recentRequests = await _dbContext.PasswordRecoveryRequests
                .Where(request => request.RequestedAt > cutoff &&
                                  (request.EmailHash == emailHash || request.OriginHash == originHash))
                .ToListAsync(cancellationToken);

            var emailAllowed = PasswordRecoveryRatePolicy.CanRequest(
                recentRequests.Where(request => request.EmailHash == emailHash).Select(request => request.RequestedAt),
                now);
            var originAllowed = PasswordRecoveryRatePolicy.CanRequest(
                recentRequests.Where(request => request.OriginHash == originHash).Select(request => request.RequestedAt),
                now);

            if (!emailAllowed || !originAllowed)
                throw new TooManyRequestsException("Se alcanzo el limite temporal de solicitudes.");

            _dbContext.PasswordRecoveryRequests.Add(new PasswordRecoveryRequest(emailHash, originHash, now));
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private async Task ReplaceWithPendingChallengeAsync(
            PasswordRecoveryChallenge challenge,
            DateTime now,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            await AcquireLockAsync($"password-recovery:{challenge.UserId}", cancellationToken);

            var obsoleteChallenges = _dbContext.PasswordRecoveryChallenges
                .Where(item => item.UserId == challenge.UserId &&
                               item.IssuedAt <= now.AddDays(-1));
            if (_dbContext.Database.IsRelational())
                await obsoleteChallenges.ExecuteDeleteAsync(cancellationToken);
            else
                _dbContext.PasswordRecoveryChallenges.RemoveRange(
                    await obsoleteChallenges.ToListAsync(cancellationToken));

            var previousChallenges = await _dbContext.PasswordRecoveryChallenges
                .Where(item => item.UserId == challenge.UserId &&
                               item.InvalidatedAt == null &&
                               item.ConsumedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var previousChallenge in previousChallenges)
                previousChallenge.Invalidate(now);

            _dbContext.PasswordRecoveryChallenges.Add(challenge);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private async Task InvalidateChallengeAsync(
            PasswordRecoveryChallenge challenge,
            DateTime invalidatedAt,
            CancellationToken cancellationToken)
        {
            challenge.Invalidate(invalidatedAt);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task AcquireLockAsync(string key, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))",
                cancellationToken);
        }

        private string HashValue(string value)
        {
            if (string.IsNullOrWhiteSpace(_settings.CodePepper))
                throw new InvalidOperationException("PasswordRecovery:CodePepper no esta configurado.");

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.CodePepper));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        private static bool HashesMatch(string expected, string supplied)
        {
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(expected),
                Convert.FromHexString(supplied));
        }

        private static string GenerateCode()
        {
            Span<char> code = stackalloc char[6];
            for (var index = 0; index < code.Length; index++)
                code[index] = CodeCharacters[RandomNumberGenerator.GetInt32(CodeCharacters.Length)];

            return new string(code);
        }
    }
}
