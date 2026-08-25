namespace VirtualBuddy.Application.Common.Interfaces
{
    public interface IPasswordRecoveryService
    {
        Task RequestCodeAsync(string email, string origin, CancellationToken cancellationToken = default);
        Task ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);
    }
}
