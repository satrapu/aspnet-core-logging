using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains unit tests targeting <see cref="LoggingMiddleware"/> class.
    /// </summary>
    public class LoggingMiddlewareTests
    {
        [Theory]
        [ClassData(typeof(ConstructorTestData))]
        public void Constructor_WhenInvokedWithAtLeastOneNullParameter_MustThrowException(RequestDelegate requestDelegate
                                                                                        , IHttpContextLoggingHandler httpContextLoggingHandler
                                                                                        , IHttpObjectConverter httpObjectConverter
                                                                                        , ILogger<LoggingMiddleware> logger)
        {
            var exception =
                Record.Exception(() => new LoggingMiddleware(requestDelegate
                                                                   , httpContextLoggingHandler
                                                                   , httpObjectConverter
                                                                   , logger));

            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WhenInvokedWithValidParameters_MustSucceed()
        {
            var requestDelegateMock = new Mock<RequestDelegate>();
            var httpContextLoggingHandlerMock = new Mock<IHttpContextLoggingHandler>();
            var httpObjectConverterMock = new Mock<IHttpObjectConverter>();
            var loggerMock = new Mock<ILogger<LoggingMiddleware>>();

            var loggingMiddleware = new LoggingMiddleware(requestDelegateMock.Object
                                                        , httpContextLoggingHandlerMock.Object
                                                        , httpObjectConverterMock.Object
                                                        , loggerMock.Object);

            loggingMiddleware.Should().NotBeNull();
        }

        /// <summary>
        /// Contains test data to be used by the parameterized unit tests from <see cref="LoggingMiddlewareTests"/> class.
        /// </summary>
        private class ConstructorTestData : TheoryData<RequestDelegate
                                                     , IHttpContextLoggingHandler
                                                     , IHttpObjectConverter
                                                     , ILogger<LoggingMiddleware>>
        {
            #pragma warning disable S1144 // Unused private types or members should be removed
            public ConstructorTestData()
            {
                // Mocking a delegate: https://dogschasingsquirrels.com/2018/05/21/mocking-delegates-with-moq/.
                var requestDelegateMock = new Mock<RequestDelegate>();
                var httpContextLoggingHandlerMock = new Mock<IHttpContextLoggingHandler>();
                var httpObjectConverterMock = new Mock<IHttpObjectConverter>();

                AddRow(requestDelegateMock.Object
                     , null
                     , null
                     , null);

                AddRow(requestDelegateMock.Object
                     , httpContextLoggingHandlerMock.Object
                     , null
                     , null);

                AddRow(requestDelegateMock.Object
                     , httpContextLoggingHandlerMock.Object
                     , httpObjectConverterMock.Object
                     , null);

                AddRow(null, null, null, null);
            }
            #pragma warning restore S1144 // Unused private types or members should be removed
        }
    }
}
