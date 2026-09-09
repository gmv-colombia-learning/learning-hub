using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace VirtualBuddy.Infraestructure.Services
{
    internal sealed class AzureOpenAIDevelopmentCertificateValidator
    {
        private readonly ILogger<AzureOpenAIDevelopmentCertificateValidator> _logger;
        private int _warningLogged;

        public AzureOpenAIDevelopmentCertificateValidator(
            ILogger<AzureOpenAIDevelopmentCertificateValidator> logger)
        {
            _logger = logger;
        }

        public bool Validate(
            HttpRequestMessage _,
            X509Certificate2? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            var chainStatuses = chain?.ChainStatus.Select(status => status.Status).ToArray();
            var isAccepted = IsAccepted(
                certificate is not null,
                chain is not null,
                sslPolicyErrors,
                chainStatuses);

            if (isAccepted && sslPolicyErrors != SslPolicyErrors.None &&
                Interlocked.Exchange(ref _warningLogged, 1) == 0)
            {
                _logger.LogWarning(
                    "Azure OpenAI Development acepto una cadena TLS cuya revocacion no pudo determinarse. " +
                    "Este workaround es temporal y esta limitado a este cliente.");
            }

            return isAccepted;
        }

        internal static bool IsAccepted(
            bool certificateAvailable,
            bool chainAvailable,
            SslPolicyErrors sslPolicyErrors,
            IReadOnlyCollection<X509ChainStatusFlags>? chainStatuses)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return certificateAvailable && chainAvailable;

            if (!certificateAvailable || !chainAvailable ||
                sslPolicyErrors != SslPolicyErrors.RemoteCertificateChainErrors ||
                chainStatuses is null || chainStatuses.Count == 0)
            {
                return false;
            }

            return chainStatuses.All(status => status == X509ChainStatusFlags.RevocationStatusUnknown);
        }
    }
}
