using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using VirtualBuddy.Infraestructure.Services;
using VirtualBuddy.Infraestructure.Util;
using Xunit;

namespace VirtualBuddy.Test.Infraestructure
{
    public class ResendEmailSenderTests
    {
        [Fact]
        public async Task SendRecoveryCode_ShouldSendExpectedRequestToResend()
        {
            var handler = new RecordingHandler(HttpStatusCode.OK);
            var sender = CreateSender(handler);

            await sender.SendRecoveryCodeAsync(
                "user@example.com",
                "ABC123",
                TimeSpan.FromMinutes(15));

            handler.Method.Should().Be(HttpMethod.Post);
            handler.RequestUri.Should().Be("https://api.resend.com/emails");
            handler.AuthorizationScheme.Should().Be("Bearer");
            handler.AuthorizationParameter.Should().Be("test-api-key");

            using var payload = JsonDocument.Parse(handler.Content!);
            var root = payload.RootElement;
            root.GetProperty("from").GetString().Should().Be("VirtualBuddy <sender@example.com>");
            root.GetProperty("to")[0].GetString().Should().Be("user@example.com");
            root.GetProperty("subject").GetString().Should().Be("Codigo de recuperacion de VirtualBuddy");
            root.GetProperty("text").GetString().Should().Contain("ABC123").And.Contain("15 minutos");
        }

        [Fact]
        public async Task SendPasswordChanged_ShouldSendExpectedNotice()
        {
            var handler = new RecordingHandler(HttpStatusCode.OK);
            var sender = CreateSender(handler);

            await sender.SendPasswordChangedAsync("user@example.com");

            using var payload = JsonDocument.Parse(handler.Content!);
            var root = payload.RootElement;
            root.GetProperty("subject").GetString().Should().Be("Contrasena restablecida en VirtualBuddy");
            root.GetProperty("text").GetString().Should().Be(
                "La contrasena de su cuenta fue restablecida correctamente. " +
                "Si no realizo este cambio, contacte al equipo de soporte.");
        }

        [Fact]
        public async Task Send_WhenResendRejectsRequest_ShouldPropagateFailure()
        {
            var handler = new RecordingHandler(HttpStatusCode.Unauthorized);
            var sender = CreateSender(handler);

            var act = () => sender.SendRecoveryCodeAsync(
                "user@example.com",
                "ABC123",
                TimeSpan.FromMinutes(15));

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        private static ResendEmailSender CreateSender(HttpMessageHandler handler)
        {
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.resend.com/")
            };
            var settings = Options.Create(new ResendSettings
            {
                ApiKey = "test-api-key",
                SenderEmail = "sender@example.com",
                SenderName = "VirtualBuddy"
            });

            return new ResendEmailSender(client, settings);
        }

        private sealed class RecordingHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;

            public HttpMethod? Method { get; private set; }
            public string? RequestUri { get; private set; }
            public string? AuthorizationScheme { get; private set; }
            public string? AuthorizationParameter { get; private set; }
            public string? Content { get; private set; }

            public RecordingHandler(HttpStatusCode statusCode)
            {
                _statusCode = statusCode;
            }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                Method = request.Method;
                RequestUri = request.RequestUri?.ToString();
                AuthorizationScheme = request.Headers.Authorization?.Scheme;
                AuthorizationParameter = request.Headers.Authorization?.Parameter;
                Content = await request.Content!.ReadAsStringAsync(cancellationToken);

                return new HttpResponseMessage(_statusCode);
            }
        }
    }
}
