namespace Todo.Telemetry.Http
{
    using System;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Contains unit tests targeting <see cref="HttpLoggingMiddleware"/> class.
    /// </summary>
    [TestFixture]
    public class HttpLoggingMiddlewareTests
    {
        /// <summary>
        /// Tests the constructor of <see cref="HttpLoggingMiddleware"/> class.
        /// </summary>
        [Test]
        public void Constructor_WhenInvokedWithValidParameters_MustSucceed()
        {
            // Arrange
            var requestDelegateMock = new Mock<RequestDelegate>();
            var httpContextLoggingHandlerMock = new Mock<IHttpContextLoggingHandler>();
            var httpObjectConverterMock = new Mock<IHttpObjectConverter>();
            var loggerMock = new Mock<ILogger<HttpLoggingMiddleware>>();

            // Act
            var loggingMiddleware = new HttpLoggingMiddleware(requestDelegateMock.Object
                , httpContextLoggingHandlerMock.Object
                , httpObjectConverterMock.Object
                , loggerMock.Object);

            // Assert
            loggingMiddleware.Should().NotBeNull("all constructor parameters are valid");
        }

        /// <summary>
        /// Tests <see cref="HttpLoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_WhenLoggingIsDisabled_MustSucceed()
        {
            // Arrange
            var requestDelegateMock = new Mock<RequestDelegate>();

            var httpContextLoggingHandlerMock = new Mock<IHttpContextLoggingHandler>();
            httpContextLoggingHandlerMock.Setup(x => x.ShouldLog(It.IsAny<HttpContext>()))
                .Returns(false);

            var httpObjectConverterMock = new Mock<IHttpObjectConverter>();
            var loggerMock = new Mock<ILogger<HttpLoggingMiddleware>>();

            var loggingMiddleware = new HttpLoggingMiddleware(requestDelegateMock.Object
                , httpContextLoggingHandlerMock.Object
                , httpObjectConverterMock.Object
                , loggerMock.Object);

            // Act
            Func<Task> invoke = async () => await loggingMiddleware.Invoke(new DefaultHttpContext());

            // Assert
            await invoke.Should().NotThrowAsync("logging middleware was built using correct values");
        }

        /// <summary>
        /// Tests <see cref="HttpLoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_WhenLoggingIsEnabled_MustSucceed()
        {
            // Arrange
            var requestDelegateMock = new Mock<RequestDelegate>();

            var httpContextLoggingHandlerMock = new Mock<IHttpContextLoggingHandler>();
            httpContextLoggingHandlerMock.Setup(x => x.ShouldLog(It.IsAny<HttpContext>()))
                .Returns(true);

            var httpObjectConverterMock = new Mock<IHttpObjectConverter>();
            var loggerMock = new Mock<ILogger<HttpLoggingMiddleware>>();

            var loggingMiddleware = new HttpLoggingMiddleware(requestDelegateMock.Object
                , httpContextLoggingHandlerMock.Object
                , httpObjectConverterMock.Object
                , loggerMock.Object);

            // Act
            Func<Task> invoke = async () => await loggingMiddleware.Invoke(new DefaultHttpContext());

            // Assert
            await invoke.Should().NotThrowAsync("logging middleware was built using correct values");
        }
    }
}
