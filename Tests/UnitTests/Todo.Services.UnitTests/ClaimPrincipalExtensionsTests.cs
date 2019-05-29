using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using Todo.Services.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="ClaimPrincipalExtensions"/> class.
    /// </summary>
    public class ClaimPrincipalExtensionsTests : IDisposable
    {
        private readonly XunitLoggerProvider xunitLoggerProvider;
        private readonly ILogger logger;

        public ClaimPrincipalExtensionsTests(ITestOutputHelper testOutputHelper)
        {
            xunitLoggerProvider = new XunitLoggerProvider(testOutputHelper);
            logger = xunitLoggerProvider.CreateLogger<ClaimPrincipalExtensionsTests>();
        }

        public void Dispose()
        {
            xunitLoggerProvider.Dispose();
        }

        /// <summary>
        /// Tests <see cref="ClaimPrincipalExtensions.GetUserId"/> method.
        /// </summary>
        [Fact]
        public void GetUserById_UsingNullAsClaimsPrincipal_MustThrowException()
        {
            try
            {
                logger.LogMethodEntered();

                // Arrange
                ClaimsPrincipal nullClaimsPrincipal = null;

                // Act
                // ReSharper disable once ExpressionIsAlwaysNull
                // ReSharper disable once InvokeAsExtensionMethod
                var exception = Record.Exception(() => ClaimPrincipalExtensions.GetUserId(nullClaimsPrincipal));

                // Assert
                exception.Should().NotBeNull();
            }
            finally
            {
                logger.LogMethodExited();
            }
        }
    }
}
