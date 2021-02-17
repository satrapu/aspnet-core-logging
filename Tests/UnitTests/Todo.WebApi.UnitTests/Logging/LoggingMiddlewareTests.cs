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
    /// Contains unit tests targeting <see cref="LoggingMiddleware"/> class.
    /// </summary>
    [TestFixture]
    public class LoggingMiddlewareTests
    {
        /// <summary>
        /// Tests the constructor of <see cref="LoggingMiddleware"/> class.
        /// </summary>
        [Test]
        public void Constructor_WhenInvokedWithValidParameters_MustSucceed()
        {
            // Arrange
            var requestDelegateMock = new Mock<RequestDelegate>();
            var httpContextLoggingHandlerMock = new Mock<IHttpContextLoggingHandler>();
            var httpObjectConverterMock = new Mock<IHttpObjectConverter>();
            var loggerMock = new Mock<ILogger<LoggingMiddleware>>();

            // Act
            var loggingMiddleware = new LoggingMiddleware(requestDelegateMock.Object
                , httpContextLoggingHandlerMock.Object
                , httpObjectConverterMock.Object
                , loggerMock.Object);

            // Assert
            loggingMiddleware.Should().NotBeNull();
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_WhenLoggingIsDisabled_MustSucceed()
        {
            try
            {
                // Arrange
                var requestDelegateMock = new Mock<RequestDelegate>();

                var httpContextLoggingHandlerMock = new Mock<IHttpContextLoggingHandler>();
                httpContextLoggingHandlerMock.Setup(x => x.ShouldLog(It.IsAny<HttpContext>()))
                    .Returns(false);

                var httpObjectConverterMock = new Mock<IHttpObjectConverter>();
                var loggerMock = new Mock<ILogger<LoggingMiddleware>>();

                var loggingMiddleware = new LoggingMiddleware(requestDelegateMock.Object
                    , httpContextLoggingHandlerMock.Object
                    , httpObjectConverterMock.Object
                    , loggerMock.Object);

                // Act
                await loggingMiddleware.Invoke(new DefaultHttpContext());
            }
            catch (Exception unexpectedException)
            {
                // Assert
                Assert.Fail(
                    $"Invoke method shouldn't have failed, but alas: {unexpectedException.Message}\n{unexpectedException.StackTrace}");
            }
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_WhenLoggingIsEnabled_MustSucceed()
        {
            try
            {
                // Arrange
                var requestDelegateMock = new Mock<RequestDelegate>();

                var httpContextLoggingHandlerMock = new Mock<IHttpContextLoggingHandler>();
                httpContextLoggingHandlerMock.Setup(x => x.ShouldLog(It.IsAny<HttpContext>()))
                    .Returns(true);

                var httpObjectConverterMock = new Mock<IHttpObjectConverter>();
                var loggerMock = new Mock<ILogger<LoggingMiddleware>>();

                var loggingMiddleware = new LoggingMiddleware(requestDelegateMock.Object
                    , httpContextLoggingHandlerMock.Object
                    , httpObjectConverterMock.Object
                    , loggerMock.Object);

                // Act
                await loggingMiddleware.Invoke(new DefaultHttpContext());
            }
            catch (Exception unexpectedException)
            {
                // Assert
                Assert.Fail(
                    $"Invoke method shouldn't have failed, but alas: {unexpectedException.Message}\n{unexpectedException.StackTrace}");
            }
        }
    }
}