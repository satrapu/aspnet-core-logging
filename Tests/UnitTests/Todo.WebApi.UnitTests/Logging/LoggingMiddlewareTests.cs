namespace Todo.WebApi.Logging
{
    using System;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

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
            loggingMiddleware.Should().NotBeNull("all constructor parameters are valid");
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public void Invoke_WhenLoggingIsDisabled_MustSucceed()
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
            Func<Task> invoke = async () => await loggingMiddleware.Invoke(new DefaultHttpContext());

            // Assert
            invoke.Should().NotThrow("logging middleware was built using correct values");
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public void Invoke_WhenLoggingIsEnabled_MustSucceed()
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
            Func<Task> invoke = async () => await loggingMiddleware.Invoke(new DefaultHttpContext());

            // Assert
            invoke.Should().NotThrow("logging middleware was built using correct values");
        }
    }
}
