using Microsoft.Extensions.Options;
using Resend;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Infraestructure.Util;

namespace VirtualBuddy.Infraestructure.Services
{
    public class ResendEmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly ResendSettings _settings;

        public ResendEmailSender(
            HttpClient httpClient,
            IOptions<ResendSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public Task SendRecoveryCodeAsync(
            string recipient,
            string code,
            TimeSpan validity,
            CancellationToken cancellationToken = default)
        {
            const string subject = "Codigo de recuperacion de VirtualBuddy";
            var body = $"Su codigo de recuperacion es: {code}\n\n" +
                       $"El codigo es valido durante {(int)validity.TotalMinutes} minutos. " +
                       "Si no solicito este cambio, ignore este mensaje.";

            return SendAsync(recipient, subject, body, cancellationToken);
        }

        public Task SendPasswordChangedAsync(
            string recipient,
            CancellationToken cancellationToken = default)
        {
            const string subject = "Contrasena restablecida en VirtualBuddy";
            const string body = "La contrasena de su cuenta fue restablecida correctamente. " +
                                "Si no realizo este cambio, contacte al equipo de soporte.";

            return SendAsync(recipient, subject, body, cancellationToken);
        }

        private async Task SendAsync(
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken)
        {

            IResend resend = ResendClient.Create(_settings.ApiKey);

            var response = await resend.EmailSendAsync(new EmailMessage()
            {
                From = _settings.SenderEmail,
                To = recipient,
                Subject = subject,
                HtmlBody = body
            });
        }
    }
}
