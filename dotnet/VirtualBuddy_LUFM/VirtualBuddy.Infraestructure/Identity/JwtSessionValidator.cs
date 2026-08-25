using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace VirtualBuddy.Infraestructure.Identity
{
    public class JwtSessionValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtSessionValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> IsValidAsync(ClaimsPrincipal principal)
        {
            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                         principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var versionClaim = principal.FindFirst("session_version")?.Value;
            var tokenVersion = int.TryParse(versionClaim, out var parsedVersion) ? parsedVersion : 0;

            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            return user != null && user.SessionVersion == tokenVersion;
        }
    }
}
