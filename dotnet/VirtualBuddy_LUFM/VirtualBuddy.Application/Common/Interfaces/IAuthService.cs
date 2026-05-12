using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;

namespace VirtualBuddy.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    }
}
