using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

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
                // Arrange & Act
                // ReSharper disable once ObjectCreationAsStatement
                new LoggingService(null);

                // Assert
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
                // Arrange
                var loggerMock = new Mock<ILogger<LoggingService>>();
                var loggingService = new LoggingService(loggerMock.Object);

                // Act
                loggingService.ShouldLog(null);
                Assert.Fail("Must not log using null HTTP context");
            }
            catch (Exception expectedException)
            {
                // Assert
                expectedException.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<ArgumentNullException>();
            }
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessageAsync(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public async Task ToLogMessageAsync_UsingNullHttpRequest_MustThrowException()
        {
            // Arrange
            HttpRequest httpRequest = null;

            try
            {
                var loggerMock = new Mock<ILogger<LoggingService>>();
                var loggingService = new LoggingService(loggerMock.Object);

                // ReSharper disable once ExpressionIsAlwaysNull
                // Act
                await loggingService.ToLogMessageAsync(httpRequest);
                Assert.Fail("Must not create log message using null HTTP request");
            }
            catch (Exception expectedException)
            {
                // Assert
                expectedException.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<ArgumentNullException>()
                    .And.Subject.As<ArgumentNullException>().ParamName.Should().Be(nameof(httpRequest));
            }
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessageAsync(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public async Task ToLogMessageAsync_UsingNullHttpResponse_MustThrowException()
        {
            // Arrange
            HttpResponse httpResponse = null;

            try
            {
                var loggerMock = new Mock<ILogger<LoggingService>>();
                var loggingService = new LoggingService(loggerMock.Object);

                // Act
                // ReSharper disable once ExpressionIsAlwaysNull
                await loggingService.ToLogMessageAsync(httpResponse);
                Assert.Fail("Must not create log message using null HTTP response");
            }
            catch (Exception expectedException)
            {
                // Assert
                expectedException.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<ArgumentNullException>()
                    .And.Subject.As<ArgumentNullException>().ParamName.Should().Be(nameof(httpResponse));
            }
        }
    }
}