using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;

namespace VirtualBuddy.Application.Auth.UseCases
{
    public class Register
    {
        private readonly IAuthService _authService;

        public Register(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<AuthResponseDto> Execute(RegisterRequestDto request)
        {
            return await _authService.RegisterAsync(request);
        }
    }
}
