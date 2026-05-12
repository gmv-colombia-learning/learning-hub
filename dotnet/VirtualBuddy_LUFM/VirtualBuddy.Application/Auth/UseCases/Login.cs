using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;

namespace VirtualBuddy.Application.Auth.UseCases
{
    public class Login
    {
        private readonly IAuthService _authService;

        public Login(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<AuthResponseDto> Execute(LoginRequestDto request)
        {
            return await _authService.LoginAsync(request);
        }
    }
}
