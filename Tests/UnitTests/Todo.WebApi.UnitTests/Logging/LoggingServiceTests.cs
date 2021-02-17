﻿using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using FluentAssertions.Common;

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
            // Arrange
            ILogger<LoggingService> logger = null;

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once ExpressionIsAlwaysNull
            Action createServiceAction = () => new LoggingService(logger);

            // Assert
            createServiceAction.Should()
                .ThrowExactly<ArgumentNullException>("must not create instance using null argument")
                .And.ParamName.IsSameOrEqualTo(nameof(logger));
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ShouldLog"/> method.
        /// </summary>
        [Test]
        public void ShouldLog_UsingNullHttpContext_MustThrowException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoggingService>>();
            var loggingService = new LoggingService(loggerMock.Object);
            HttpContext httpContext = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Action shouldLogAction = () => loggingService.ShouldLog(httpContext);

            // Assert
            shouldLogAction.Should().ThrowExactly<ArgumentNullException>("HTTP context is null")
                .And.ParamName.IsSameOrEqualTo(nameof(httpContext));
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessageAsync(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public void ToLogMessageAsync_UsingNullHttpRequest_MustThrowException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoggingService>>();
            var loggingService = new LoggingService(loggerMock.Object);
            HttpRequest httpRequest = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> toLogMessageAsyncCall = async () => await loggingService.ToLogMessageAsync(httpRequest);

            // Assert
            toLogMessageAsyncCall.Should().ThrowExactly<ArgumentNullException>()
                .And.ParamName.Should().Be(nameof(httpRequest));
        }

        /// <summary>
        /// Tests the <see cref="LoggingService.ToLogMessageAsync(HttpRequest)"/> method.
        /// </summary>
        [Test]
        public void ToLogMessageAsync_UsingNullHttpResponse_MustThrowException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoggingService>>();
            var loggingService = new LoggingService(loggerMock.Object);
            HttpResponse httpResponse = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> toLogMessageAsyncCall = async () => await loggingService.ToLogMessageAsync(httpResponse);

            // Assert
            toLogMessageAsyncCall.Should().ThrowExactly<ArgumentNullException>()
                .And.ParamName.Should().Be(nameof(httpResponse));
        }
    }
}