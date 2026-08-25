using VirtualBuddy.Application.Auth.UseCases;

namespace VirtualBuddy.Application.Auth
{
    public class AuthFacade
    {
        public Login Login { get; }
        public Register Register { get; }
        public RequestPasswordRecovery RequestPasswordRecovery { get; }
        public ResetPassword ResetPassword { get; }

        public AuthFacade(
            Login login,
            Register register,
            RequestPasswordRecovery requestPasswordRecovery,
            ResetPassword resetPassword)
        {
            Login = login;
            Register = register;
            RequestPasswordRecovery = requestPasswordRecovery;
            ResetPassword = resetPassword;
        }
    }
}
