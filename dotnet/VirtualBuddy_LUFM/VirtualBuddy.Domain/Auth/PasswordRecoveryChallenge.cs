using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Domain.Auth
{
    public class PasswordRecoveryChallenge : Entity
    {
        public const int MaximumFailedAttempts = 3;

        public string UserId { get; private set; } = null!;
        public string CodeHash { get; private set; } = null!;
        public DateTime IssuedAt { get; private set; }
        public DateTime? ActivatedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public DateTime? InvalidatedAt { get; private set; }
        public DateTime? ConsumedAt { get; private set; }
        public int FailedAttempts { get; private set; }
        public Guid ConcurrencyStamp { get; private set; }

        private PasswordRecoveryChallenge() { }

        private PasswordRecoveryChallenge(string userId, string codeHash, DateTime issuedAt)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(codeHash))
                throw new ValidationException("El desafio de recuperacion no es valido.");

            UserId = userId;
            CodeHash = codeHash;
            IssuedAt = issuedAt;
            ConcurrencyStamp = Guid.NewGuid();
        }

        public static PasswordRecoveryChallenge CreatePending(string userId, string codeHash, DateTime issuedAt)
        {
            return new PasswordRecoveryChallenge(userId, codeHash, issuedAt);
        }

        public bool IsActive(DateTime now)
        {
            return ActivatedAt.HasValue &&
                   ExpiresAt > now &&
                   !InvalidatedAt.HasValue &&
                   !ConsumedAt.HasValue;
        }

        public void Activate(DateTime activatedAt, TimeSpan lifetime)
        {
            if (InvalidatedAt.HasValue || ConsumedAt.HasValue || ActivatedAt.HasValue)
                throw new ConflictException("El desafio de recuperacion no puede activarse.");

            ActivatedAt = activatedAt;
            ExpiresAt = activatedAt.Add(lifetime);
            Touch();
        }

        public void RegisterFailedAttempt(DateTime attemptedAt)
        {
            if (!IsActive(attemptedAt))
                throw new ValidationException("El codigo no es valido o ha expirado.");

            FailedAttempts++;
            if (FailedAttempts >= MaximumFailedAttempts)
                InvalidatedAt = attemptedAt;

            Touch();
        }

        public void Consume(DateTime consumedAt)
        {
            if (!IsActive(consumedAt))
                throw new ValidationException("El codigo no es valido o ha expirado.");

            ConsumedAt = consumedAt;
            Touch();
        }

        public void Invalidate(DateTime invalidatedAt)
        {
            if (InvalidatedAt.HasValue || ConsumedAt.HasValue)
                return;

            InvalidatedAt = invalidatedAt;
            Touch();
        }

        private void Touch()
        {
            ConcurrencyStamp = Guid.NewGuid();
        }
    }
}
