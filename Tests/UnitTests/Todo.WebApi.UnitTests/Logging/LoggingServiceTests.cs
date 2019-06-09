using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains unit tests targeting <see cref="LoggingService"/> class.
    /// </summary>
    [TestFixture]
    public class LoggingServiceTests
    {
        /// <summary>
        /// Tests the constructor of <see cref="LoggingService"/> class.
        /// </summary>
        [Test]
        public void Constructor_UsingNullLogger_MustThrowException()
        {
            try
            {
                // ReSharper disable once UnusedVariable
                var loggingService = new LoggingService(null);
                Assert.Fail("Must not create instance using null argument");
            }
            catch (Exception expectedException)
            {
                expectedException.Should()
                                 .NotBeNull()
                                 .And.BeAssignableTo<ArgumentNullException>();
            }
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ShouldLog"/> method.
        /// </summary>
        [Test]
        public void ShouldLog_UsingNullHttpContext_MustThrowException()
        {
            try
            {
                var loggerMock = new Mock<ILogger<LoggingService>>();
                var loggingService = new LoggingService(loggerMock.Object);
                loggingService.ShouldLog(null);
                Assert.Fail("Must not log using null HTTP context");
            }
            catch (Exception expectedException)
            {
                expectedException.Should()
                                 .NotBeNull()
                                 .And.BeAssignableTo<ArgumentNullException>();
            }
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessage(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public void ToLogMessage_UsingNullHttpRequest_MustThrowException()
        {
            HttpRequest httpRequest = null;

            try
            {
                var loggerMock = new Mock<ILogger<LoggingService>>();
                var loggingService = new LoggingService(loggerMock.Object);
                // ReSharper disable once ExpressionIsAlwaysNull
                loggingService.ToLogMessage(httpRequest);
                Assert.Fail("Must not create log message using null HTTP request");
            }
            catch (Exception expectedException)
            {
                expectedException.Should()
                                 .NotBeNull()
                                 .And.BeAssignableTo<ArgumentNullException>()
                                 .And.Subject.As<ArgumentNullException>().ParamName.Should().Be(nameof(httpRequest));
            }
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessage(HttpResponse)"/> method.
        /// </summary>
        [Test]
        public void ToLogMessage_UsingNullHttpResponse_MustThrowException()
        {
            HttpResponse httpResponse = null;

            try
            {
                var loggerMock = new Mock<ILogger<LoggingService>>();
                var loggingService = new LoggingService(loggerMock.Object);
                // ReSharper disable once ExpressionIsAlwaysNull
                loggingService.ToLogMessage(httpResponse);
                Assert.Fail("Must not create log message using null HTTP response");
            }
            catch (Exception expectedException)
            {
                expectedException.Should()
                                 .NotBeNull()
                                 .And.BeAssignableTo<ArgumentNullException>()
                                 .And.Subject.As<ArgumentNullException>().ParamName.Should().Be(nameof(httpResponse));
            }
        }
    }
}
