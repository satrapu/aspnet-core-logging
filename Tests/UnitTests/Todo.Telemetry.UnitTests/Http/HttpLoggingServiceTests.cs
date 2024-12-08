namespace Todo.Telemetry.Http
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Contains unit tests targeting <see cref="HttpLoggingService"/> class.
    /// </summary>
    [TestFixture]
    public class HttpLoggingServiceTests
    {
        /// <summary>
        /// Tests the constructor of <see cref="HttpLoggingService"/> class.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1806:Do not ignore method results",
            Justification = "Newly created instance is discarded for testing purposes")]
        public void Constructor_UsingNullLogger_MustThrowException()
        {
            // Arrange
            ILogger<HttpLoggingService> logger = null;

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once ExpressionIsAlwaysNull
            Action invokeConstructor = () => new HttpLoggingService(logger);

            // Assert
            invokeConstructor
                .Should()
                .ThrowExactly<ArgumentNullException>("must not create instance using null argument")
                .WithParameterName(nameof(logger));
        }

        /// <summary>
        /// Tests the <see cref="HttpLoggingService.ShouldLog"/> method.
        /// </summary>
        [Test]
        public void ShouldLog_UsingNullHttpContext_MustThrowException()
        {
            // Arrange
            HttpContext httpContext = null;
            HttpLoggingService classUnderTest = new(logger: GetLogger());

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Action shouldLogAction = () => classUnderTest.ShouldLog(httpContext);

            // Assert
            shouldLogAction
                .Should()
                .ThrowExactly<ArgumentNullException>("HTTP context is null")
                .WithParameterName(nameof(httpContext));
        }

        /// <summary>
        /// Tests the <see cref="HttpLoggingService.ShouldLog"/> method.
        /// </summary>
        [Test]
        public void ShouldLog_UsingDebugAsLogLevel_LogsEverything()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Accept = "application/json";

            HttpLoggingService classUnderTest = new(logger: GetLogger());

            // Act
            bool actualResult = classUnderTest.ShouldLog(httpContext);

            // Assert
            actualResult.Should().BeTrue();
        }

        /// <summary>
        /// Tests the <see cref="HttpLoggingService.ToLogMessageAsync(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public async Task ToLogMessageAsync_UsingNullHttpRequest_MustThrowException()
        {
            // Arrange
            HttpRequest httpRequest = null;
            HttpLoggingService classUnderTest = new(logger: GetLogger());

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> toLogMessageAsyncCall = async () => await classUnderTest.ToLogMessageAsync(httpRequest);

            // Assert
            await toLogMessageAsyncCall
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>("HTTP context is null")
                .WithParameterName(nameof(httpRequest));
        }

        /// <summary>
        /// Tests the <see cref="HttpLoggingService.ToLogMessageAsync(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public async Task ToLogMessageAsync_UsingNullHttpResponse_MustThrowException()
        {
            // Arrange
            HttpResponse httpResponse = null;
            HttpLoggingService classUnderTest = new(logger: GetLogger());

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> toLogMessageAsyncCall = async () => await classUnderTest.ToLogMessageAsync(httpResponse);

            // Assert
            await toLogMessageAsyncCall
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>()
                .WithParameterName(nameof(httpResponse));
        }

        /// <summary>
        /// Tests the <see cref="HttpLoggingService.ToLogMessageAsync(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public async Task ToLogMessageAsync_UsingValidHttpRequest_ReturnsExpectedData()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("X-Header1", "header1");
            httpContext.Request.Headers.Append("X-Header2", "header2");

            HttpLoggingService classUnderTest = new(logger: GetLogger());

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            string actualLogMessage = await classUnderTest.ToLogMessageAsync(httpContext.Request);

            // Assert
            actualLogMessage.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Tests the <see cref="HttpLoggingService.ToLogMessageAsync(HttpResponse)"/> method.
        /// </summary>
        [Test]
        public async Task ToLogMessageAsync_UsingValidHttpResponse_ReturnsExpectedData()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Headers.Append("X-Header1", "header1");
            httpContext.Response.Headers.Append("X-Header2", "header2");

            HttpLoggingService classUnderTest = new(logger: GetLogger());

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            string actualLogMessage = await classUnderTest.ToLogMessageAsync(httpContext.Response);

            // Assert
            actualLogMessage.Should().NotBeNullOrWhiteSpace();
        }

        private static ILogger<HttpLoggingService> GetLogger()
        {
            Mock<ILogger<HttpLoggingService>> loggerMock = new();
            loggerMock
                .Setup(x => x.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);

            return loggerMock.Object;
        }
    }
}
