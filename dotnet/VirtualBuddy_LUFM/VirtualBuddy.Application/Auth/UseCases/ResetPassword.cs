using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Auth.ValueObjects;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Application.Auth.UseCases
{
    public class ResetPassword
    {
        private readonly IPasswordRecoveryService _passwordRecoveryService;

        public ResetPassword(IPasswordRecoveryService passwordRecoveryService)
        {
            _passwordRecoveryService = passwordRecoveryService;
        }

        public async Task<PasswordRecoveryResponseDto> Execute(
            ResetPasswordRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var email = new EmailAddress(request.Email);
            var code = new RecoveryCode(request.Code);

            if (string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
                throw new ValidationException("La contrasena nueva y su confirmacion son obligatorias.");

            if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
                throw new ValidationException("La contrasena nueva y su confirmacion deben coincidir.");

            await _passwordRecoveryService.ResetPasswordAsync(
                email.Value,
                code.Value,
                request.NewPassword,
                cancellationToken);

            return new PasswordRecoveryResponseDto
            {
                Message = "La contrasena fue restablecida. Inicie sesion nuevamente."
            };
        }
    }
}
