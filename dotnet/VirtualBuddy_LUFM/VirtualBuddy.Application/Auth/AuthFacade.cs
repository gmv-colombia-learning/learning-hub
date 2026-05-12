using VirtualBuddy.Application.Auth.UseCases;

namespace VirtualBuddy.Application.Auth
{
    public class AuthFacade
    {
        public Login Login { get; }
        public Register Register { get; }

        public AuthFacade(Login login, Register register)
        {
            Login = login;
            Register = register;
        }
    }
}
