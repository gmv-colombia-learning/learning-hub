using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VirtualBuddy.Api.Middleware;
using VirtualBuddy.Domain.Common.Exceptions;
using Xunit;

namespace VirtualBuddy.Test.Api
{
    public class ExceptionMiddlewareTests
    {
        [Theory]
        [InlineData(typeof(TooManyRequestsException), StatusCodes.Status429TooManyRequests)]
        [InlineData(typeof(TemporaryServiceUnavailableException), StatusCodes.Status503ServiceUnavailable)]
        public async Task Middleware_ShouldMapRecoveryErrorsToProblemDetails(
            Type exceptionType,
            int expectedStatus)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType, "generic error")!;
            var middleware = new ExceptionMiddleware(
                _ => throw exception,
                NullLogger<ExceptionMiddleware>.Instance,
                Mock.Of<IWebHostEnvironment>());
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(expectedStatus);
            context.Response.ContentType.Should().Be("application/problem+json");
        }
    }
}
