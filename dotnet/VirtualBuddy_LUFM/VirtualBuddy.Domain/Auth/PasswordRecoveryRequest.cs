using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Domain.Auth
{
    public class PasswordRecoveryRequest : Entity
    {
        public string EmailHash { get; private set; } = null!;
        public string OriginHash { get; private set; } = null!;
        public DateTime RequestedAt { get; private set; }

        private PasswordRecoveryRequest() { }

        public PasswordRecoveryRequest(string emailHash, string originHash, DateTime requestedAt)
        {
            if (string.IsNullOrWhiteSpace(emailHash) || string.IsNullOrWhiteSpace(originHash))
                throw new ValidationException("Los identificadores de la solicitud son obligatorios.");

            EmailHash = emailHash;
            OriginHash = originHash;
            RequestedAt = requestedAt;
        }
    }
}
