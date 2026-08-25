using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Auth.ValueObjects;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Application.Auth.UseCases
{
    public class RequestPasswordRecovery
    {
        private const string GenericMessage = "Si existe una cuenta asociada, se enviaran instrucciones de recuperacion.";
        private readonly IPasswordRecoveryService _passwordRecoveryService;

        public RequestPasswordRecovery(IPasswordRecoveryService passwordRecoveryService)
        {
            _passwordRecoveryService = passwordRecoveryService;
        }

        public async Task<PasswordRecoveryResponseDto> Execute(
            ForgotPasswordRequestDto request,
            string origin,
            CancellationToken cancellationToken = default)
        {
            var email = new EmailAddress(request.Email);
            if (string.IsNullOrWhiteSpace(origin))
                throw new ValidationException("No se pudo determinar el origen de la solicitud.");

            await _passwordRecoveryService.RequestCodeAsync(email.Value, origin, cancellationToken);

            return new PasswordRecoveryResponseDto { Message = GenericMessage };
        }
    }
}
