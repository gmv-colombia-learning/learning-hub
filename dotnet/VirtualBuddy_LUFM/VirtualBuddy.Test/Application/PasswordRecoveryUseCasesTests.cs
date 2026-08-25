using FluentAssertions;
using Moq;
using VirtualBuddy.Application.Auth.UseCases;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Domain.Common.Exceptions;
using Xunit;

namespace VirtualBuddy.Test.Application
{
    public class PasswordRecoveryUseCasesTests
    {
        private readonly Mock<IPasswordRecoveryService> _service = new();

        [Fact]
        public async Task Request_WhenValid_ShouldReturnGenericResponse()
        {
            var useCase = new RequestPasswordRecovery(_service.Object);

            var response = await useCase.Execute(
                new ForgotPasswordRequestDto { Email = "user@example.com" },
                "127.0.0.1");

            response.Message.Should().Be("Si existe una cuenta asociada, se enviaran instrucciones de recuperacion.");
            _service.Verify(service => service.RequestCodeAsync(
                "user@example.com",
                "127.0.0.1",
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Request_WhenEmailIsInvalid_ShouldNotCallService()
        {
            var useCase = new RequestPasswordRecovery(_service.Object);

            var act = () => useCase.Execute(
                new ForgotPasswordRequestDto { Email = "invalid-email" },
                "127.0.0.1");

            await act.Should().ThrowAsync<ValidationException>();
            _service.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Reset_WhenPasswordsDoNotMatch_ShouldNotConsumeCode()
        {
            var useCase = new ResetPassword(_service.Object);
            var request = new ResetPasswordRequestDto
            {
                Email = "user@example.com",
                Code = "ABC123",
                NewPassword = "NewPassword1",
                ConfirmPassword = "Different1"
            };

            var act = () => useCase.Execute(request);

            await act.Should().ThrowAsync<ValidationException>();
            _service.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Reset_WhenCodeIsMalformed_ShouldNotConsumeAttempt()
        {
            var useCase = new ResetPassword(_service.Object);
            var request = new ResetPasswordRequestDto
            {
                Email = "user@example.com",
                Code = "BAD",
                NewPassword = "NewPassword1",
                ConfirmPassword = "NewPassword1"
            };

            var act = () => useCase.Execute(request);

            await act.Should().ThrowAsync<ValidationException>();
            _service.VerifyNoOtherCalls();
        }
    }
}
