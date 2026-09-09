using FluentAssertions;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using VirtualBuddy.Infraestructure.Services;
using Xunit;

namespace VirtualBuddy.Test.Infraestructure
{
    public class AzureOpenAIDevelopmentCertificateValidatorTests
    {
        [Fact]
        public void IsAccepted_ShouldAcceptCertificateWithoutTlsErrors()
        {
            var result = AzureOpenAIDevelopmentCertificateValidator.IsAccepted(
                certificateAvailable: true,
                chainAvailable: true,
                SslPolicyErrors.None,
                []);

            result.Should().BeTrue();
        }

        [Fact]
        public void IsAccepted_ShouldRejectMissingChainWithoutReportedTlsErrors()
        {
            var result = AzureOpenAIDevelopmentCertificateValidator.IsAccepted(
                certificateAvailable: true,
                chainAvailable: false,
                SslPolicyErrors.None,
                null);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsAccepted_ShouldAcceptOnlyUnknownRevocationStatuses()
        {
            var result = AzureOpenAIDevelopmentCertificateValidator.IsAccepted(
                certificateAvailable: true,
                chainAvailable: true,
                SslPolicyErrors.RemoteCertificateChainErrors,
                [
                    X509ChainStatusFlags.RevocationStatusUnknown,
                    X509ChainStatusFlags.RevocationStatusUnknown
                ]);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(SslPolicyErrors.RemoteCertificateNameMismatch)]
        [InlineData(SslPolicyErrors.RemoteCertificateNotAvailable)]
        [InlineData(SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors)]
        public void IsAccepted_ShouldRejectNonChainTlsErrors(SslPolicyErrors sslPolicyErrors)
        {
            var result = AzureOpenAIDevelopmentCertificateValidator.IsAccepted(
                certificateAvailable: true,
                chainAvailable: true,
                sslPolicyErrors,
                [X509ChainStatusFlags.RevocationStatusUnknown]);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(X509ChainStatusFlags.Revoked)]
        [InlineData(X509ChainStatusFlags.NotTimeValid)]
        [InlineData(X509ChainStatusFlags.UntrustedRoot)]
        [InlineData(X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.NotTimeValid)]
        public void IsAccepted_ShouldRejectAnyOtherChainStatus(X509ChainStatusFlags chainStatus)
        {
            var result = AzureOpenAIDevelopmentCertificateValidator.IsAccepted(
                certificateAvailable: true,
                chainAvailable: true,
                SslPolicyErrors.RemoteCertificateChainErrors,
                [chainStatus]);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void IsAccepted_ShouldRejectMissingCertificateOrChain(
            bool certificateAvailable,
            bool chainAvailable)
        {
            var result = AzureOpenAIDevelopmentCertificateValidator.IsAccepted(
                certificateAvailable,
                chainAvailable,
                SslPolicyErrors.RemoteCertificateChainErrors,
                [X509ChainStatusFlags.RevocationStatusUnknown]);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsAccepted_ShouldRejectChainErrorWithoutStatuses()
        {
            var result = AzureOpenAIDevelopmentCertificateValidator.IsAccepted(
                certificateAvailable: true,
                chainAvailable: true,
                SslPolicyErrors.RemoteCertificateChainErrors,
                []);

            result.Should().BeFalse();
        }
    }
}
