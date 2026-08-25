using Microsoft.AspNetCore.Identity;

namespace VirtualBuddy.Infraestructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public int SessionVersion { get; private set; }

        public void RevokeSessions()
        {
            SessionVersion++;
        }
    }
}
