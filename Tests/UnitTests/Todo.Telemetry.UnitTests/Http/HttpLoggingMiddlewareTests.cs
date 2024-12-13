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
            Mock<RequestDelegate> requestDelegateMock = new();
            Mock<IHttpContextLoggingHandler> httpContextLoggingHandlerMock = new();
            Mock<IHttpObjectConverter> httpObjectConverterMock = new();
            Mock<ILogger<HttpLoggingMiddleware>> loggerMock = new();

            // Act
            HttpLoggingMiddleware classUnderTest = new
            (
                requestDelegateMock.Object,
                httpContextLoggingHandlerMock.Object,
                httpObjectConverterMock.Object,
                loggerMock.Object
            );

            // Assert
            classUnderTest.Should().NotBeNull("all constructor parameters are valid");
        }

        /// <summary>
        /// Tests <see cref="HttpLoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_WhenLoggingIsDisabled_MustSucceed()
        {
            // Arrange
            Mock<RequestDelegate> requestDelegateMock = new();

            Mock<IHttpContextLoggingHandler> httpContextLoggingHandlerMock = new();
            httpContextLoggingHandlerMock.Setup(x => x.ShouldLog(It.IsAny<HttpContext>()))
                .Returns(false);

            Mock<IHttpObjectConverter> httpObjectConverterMock = new();
            Mock<ILogger<HttpLoggingMiddleware>> loggerMock = new();

            HttpLoggingMiddleware classUnderTest = new
            (
                requestDelegateMock.Object,
                httpContextLoggingHandlerMock.Object,
                httpObjectConverterMock.Object,
                loggerMock.Object
            );

            // Act
            Func<Task> invoke = async () => await classUnderTest.Invoke(new DefaultHttpContext());

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
            Mock<RequestDelegate> requestDelegateMock = new();

            Mock<IHttpContextLoggingHandler> httpContextLoggingHandlerMock = new();
            httpContextLoggingHandlerMock.Setup(x => x.ShouldLog(It.IsAny<HttpContext>()))
                .Returns(true);

            Mock<IHttpObjectConverter> httpObjectConverterMock = new();
            Mock<ILogger<HttpLoggingMiddleware>> loggerMock = new();

            HttpLoggingMiddleware classUnderTest = new
            (
                requestDelegateMock.Object,
                httpContextLoggingHandlerMock.Object,
                httpObjectConverterMock.Object,
                loggerMock.Object
            );

            // Act
            Func<Task> invoke = async () => await classUnderTest.Invoke(new DefaultHttpContext());

            // Assert
            await invoke.Should().NotThrowAsync("logging middleware was built using correct values");
        }
    }
}
