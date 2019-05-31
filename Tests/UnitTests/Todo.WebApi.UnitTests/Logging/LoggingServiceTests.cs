using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains unit tests targeting <see cref="LoggingService"/> class.
    /// </summary>
    public class LoggingServiceTests
    {
        /// <summary>
        /// Tests the constructor of <see cref="LoggingService"/> class.
        /// </summary>
        [Fact]
        public void Constructor_UsingNullLogger_MustThrowException()
        {
            var exception = Record.Exception(() => new LoggingService(null));
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>();
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ShouldLog"/> method.
        /// </summary>
        [Fact]
        public void ShouldLog_UsingNullHttpContext_MustThrowException()
        {
            var loggerMock = new Mock<ILogger<LoggingService>>();
            var loggingService = new LoggingService(loggerMock.Object);

            var exception = Record.Exception(() => loggingService.ShouldLog(null));
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>();
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessage(HttpRequest)"/> method.
        /// </summary>
        [Fact]
        public void ToLogMessage_UsingNullHttpRequest_MustThrowException()
        {
            var loggerMock = new Mock<ILogger<LoggingService>>();
            var loggingService = new LoggingService(loggerMock.Object);

            var exception = Record.Exception(() => loggingService.ToLogMessage((HttpRequest)null));
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>();
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessage(HttpResponse)"/> method.
        /// </summary>
        [Fact]
        public void ToLogMessage_UsingNullHttpResponse_MustThrowException()
        {
            var loggerMock = new Mock<ILogger<LoggingService>>();
            var loggingService = new LoggingService(loggerMock.Object);

            var exception = Record.Exception(() => loggingService.ToLogMessage((HttpResponse)null));
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>();
        }
    }
}
