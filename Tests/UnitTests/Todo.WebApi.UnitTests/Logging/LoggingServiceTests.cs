using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Todo.TestInfrastructure.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains unit tests targeting <see cref="LoggingService"/> class.
    /// </summary>
    public class LoggingServiceTests: IDisposable
    {
        private readonly XunitLoggerProvider xunitLoggerProvider;
        private readonly ILogger logger;

        public LoggingServiceTests(ITestOutputHelper testOutputHelper)
        {
            xunitLoggerProvider = new XunitLoggerProvider(testOutputHelper);
            logger = xunitLoggerProvider.CreateLogger<LoggingServiceTests>();
        }

        public void Dispose()
        {
            xunitLoggerProvider.Dispose();
        }

        /// <summary>
        /// Tests the constructor of <see cref="LoggingService"/> class.
        /// </summary>
        [Fact]
        public void Constructor_UsingNullLogger_MustThrowException()
        {
            try
            {
                logger.LogMethodEntered();
                var exception = Record.Exception(() => new LoggingService(null));

                exception.Should()
                         .NotBeNull()
                         .And.BeAssignableTo<ArgumentNullException>();
            }
            finally
            {
                logger.LogMethodExited();
            }
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ShouldLog"/> method.
        /// </summary>
        [Fact]
        public void ShouldLog_UsingNullHttpContext_MustThrowException()
        {
            try
            {
                logger.LogMethodEntered();
                var loggerMock = new Mock<ILogger<LoggingService>>();
                var loggingService = new LoggingService(loggerMock.Object);

                var exception = Record.Exception(() => loggingService.ShouldLog(null));
                exception.Should()
                         .NotBeNull()
                         .And.BeAssignableTo<ArgumentNullException>();
            }
            finally
            {
                logger.LogMethodExited();
            }
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessage(HttpRequest)"/> method.
        /// </summary>
        [Fact]
        public void ToLogMessage_UsingNullHttpRequest_MustThrowException()
        {
            try
            {
                logger.LogMethodEntered();
                var loggerMock = new Mock<ILogger<LoggingService>>();
                var loggingService = new LoggingService(loggerMock.Object);

                var exception = Record.Exception(() => loggingService.ToLogMessage((HttpRequest)null));
                exception.Should()
                         .NotBeNull()
                         .And.BeAssignableTo<ArgumentNullException>();
            }
            finally
            {
                logger.LogMethodExited();
            }
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessage(HttpResponse)"/> method.
        /// </summary>
        [Fact]
        public void ToLogMessage_UsingNullHttpResponse_MustThrowException()
        {
            try
            {
                logger.LogMethodEntered();
                var loggerMock = new Mock<ILogger<LoggingService>>();
                var loggingService = new LoggingService(loggerMock.Object);

                var exception = Record.Exception(() => loggingService.ToLogMessage((HttpResponse)null));
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
