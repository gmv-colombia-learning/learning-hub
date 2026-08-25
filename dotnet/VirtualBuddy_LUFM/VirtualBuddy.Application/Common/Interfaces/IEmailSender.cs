namespace VirtualBuddy.Application.Common.Interfaces
{
    public interface IEmailSender
    {
        Task SendRecoveryCodeAsync(string recipient, string code, TimeSpan validity, CancellationToken cancellationToken = default);
        Task SendPasswordChangedAsync(string recipient, CancellationToken cancellationToken = default);
    }
}
