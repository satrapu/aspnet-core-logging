using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System;
using Todo.TestInfrastructure.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains unit tests targeting <see cref="LoggingMiddlewareExtensions"/> class.
    /// </summary>
    public class LoggingMiddlewareExtensionsTests: IDisposable
    {
        private readonly XunitLoggerProvider xunitLoggerProvider;
        private readonly ILogger logger;

        public LoggingMiddlewareExtensionsTests(ITestOutputHelper testOutputHelper)
        {
            xunitLoggerProvider = new XunitLoggerProvider(testOutputHelper);
            logger = xunitLoggerProvider.CreateLogger<LoggingMiddlewareExtensionsTests>();
        }

        public void Dispose()
        {
            xunitLoggerProvider.Dispose();
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddlewareExtensions.UseHttpLogging"/> method.
        /// </summary>
        [Fact]
        public void UseHttpLogging_UsingNullApplicationBuilder_MustThrowException()
        {
            try
            {
                logger.LogMethodEntered();
                IApplicationBuilder applicationBuilder = null;

                // ReSharper disable once ExpressionIsAlwaysNull
                var exception = Record.Exception(() => applicationBuilder.UseHttpLogging());

                exception.Should()
                         .NotBeNull()
                         .And.BeAssignableTo<ArgumentNullException>();
            }
            finally
            {
                logger.LogMethodExited();
            }
        }
    }
}
