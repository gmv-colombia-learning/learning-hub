using FluentAssertions;
using VirtualBuddy.Domain.Auth;
using VirtualBuddy.Domain.Auth.ValueObjects;
using VirtualBuddy.Domain.Common.Exceptions;
using Xunit;

namespace VirtualBuddy.Test.AuthDomain
{
    public class PasswordRecoveryDomainTests
    {
        private static readonly DateTime Now = new(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void RecoveryCode_ShouldNormalizeLowercase()
        {
            var code = new RecoveryCode("a1b2c3");

            code.Value.Should().Be("A1B2C3");
        }

        [Theory]
        [InlineData("")]
        [InlineData("ABC12")]
        [InlineData("ABC1234")]
        [InlineData("ABC-12")]
        [InlineData("ÁBC123")]
        public void RecoveryCode_WhenMalformed_ShouldThrow(string value)
        {
            var act = () => new RecoveryCode(value);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Challenge_ShouldExpireAtExactlyFifteenMinutes()
        {
            var challenge = CreateActiveChallenge();

            challenge.IsActive(Now.AddMinutes(15).AddTicks(-1)).Should().BeTrue();
            challenge.IsActive(Now.AddMinutes(15)).Should().BeFalse();
        }

        [Fact]
        public void Challenge_ShouldInvalidateOnThirdFailedAttempt()
        {
            var challenge = CreateActiveChallenge();

            challenge.RegisterFailedAttempt(Now.AddMinutes(1));
            challenge.RegisterFailedAttempt(Now.AddMinutes(2));
            challenge.IsActive(Now.AddMinutes(2)).Should().BeTrue();

            challenge.RegisterFailedAttempt(Now.AddMinutes(3));

            challenge.FailedAttempts.Should().Be(3);
            challenge.IsActive(Now.AddMinutes(3)).Should().BeFalse();
        }

        [Fact]
        public void Challenge_AfterConsumption_ShouldNotBeReusable()
        {
            var challenge = CreateActiveChallenge();
            challenge.Consume(Now.AddMinutes(1));

            var act = () => challenge.Consume(Now.AddMinutes(2));

            challenge.IsActive(Now.AddMinutes(2)).Should().BeFalse();
            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void RatePolicy_ShouldUseRollingMinuteAndHourWindows()
        {
            PasswordRecoveryRatePolicy.CanRequest(new[] { Now.AddSeconds(-59) }, Now).Should().BeFalse();
            PasswordRecoveryRatePolicy.CanRequest(new[] { Now.AddMinutes(-1) }, Now).Should().BeTrue();

            var fiveRequests = Enumerable.Range(1, 5).Select(index => Now.AddMinutes(-index * 2));
            PasswordRecoveryRatePolicy.CanRequest(fiveRequests, Now).Should().BeFalse();
        }

        private static PasswordRecoveryChallenge CreateActiveChallenge()
        {
            var challenge = PasswordRecoveryChallenge.CreatePending("user-id", "HASH", Now);
            challenge.Activate(Now, TimeSpan.FromMinutes(15));
            return challenge;
        }
    }
}
