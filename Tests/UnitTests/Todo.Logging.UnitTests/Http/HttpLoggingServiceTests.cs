namespace Todo.Logging.Http
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Common;

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
            Action createServiceAction = () => new HttpLoggingService(logger);

            // Assert
            createServiceAction
                .Should().ThrowExactly<ArgumentNullException>("must not create instance using null argument")
                .And.ParamName.IsSameOrEqualTo(nameof(logger));
        }

        /// <summary>
        /// Tests the <see cref="HttpLoggingService.ShouldLog"/> method.
        /// </summary>
        [Test]
        public void ShouldLog_UsingNullHttpContext_MustThrowException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HttpLoggingService>>();
            var loggingService = new HttpLoggingService(loggerMock.Object);
            HttpContext httpContext = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Action shouldLogAction = () => loggingService.ShouldLog(httpContext);

            // Assert
            shouldLogAction
                .Should().ThrowExactly<ArgumentNullException>("HTTP context is null")
                .And.ParamName.IsSameOrEqualTo(nameof(httpContext));
        }

        /// <summary>
        /// Tests the <see cref="HttpLoggingService.ToLogMessageAsync(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public void ToLogMessageAsync_UsingNullHttpRequest_MustThrowException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HttpLoggingService>>();
            var loggingService = new HttpLoggingService(loggerMock.Object);
            HttpRequest httpRequest = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> toLogMessageAsyncCall = async () => await loggingService.ToLogMessageAsync(httpRequest);

            // Assert
            toLogMessageAsyncCall
                .Should().ThrowExactly<ArgumentNullException>()
                .And.ParamName.Should().Be(nameof(httpRequest));
        }

        /// <summary>
        /// Tests the <see cref="HttpLoggingService.ToLogMessageAsync(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public void ToLogMessageAsync_UsingNullHttpResponse_MustThrowException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HttpLoggingService>>();
            var loggingService = new HttpLoggingService(loggerMock.Object);
            HttpResponse httpResponse = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> toLogMessageAsyncCall = async () => await loggingService.ToLogMessageAsync(httpResponse);

            // Assert
            toLogMessageAsyncCall
                .Should().ThrowExactly<ArgumentNullException>()
                .And.ParamName.Should().Be(nameof(httpResponse));
        }
    }
}
