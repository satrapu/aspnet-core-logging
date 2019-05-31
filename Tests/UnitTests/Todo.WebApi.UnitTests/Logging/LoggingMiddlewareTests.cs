using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Todo.TestInfrastructure.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains unit tests targeting <see cref="LoggingMiddleware"/> class.
    /// </summary>
    public class LoggingMiddlewareTests: IDisposable
    {
        private readonly XunitLoggerProvider xunitLoggerProvider;
        private readonly ILogger logger;

        public LoggingMiddlewareTests(ITestOutputHelper testOutputHelper)
        {
            xunitLoggerProvider = new XunitLoggerProvider(testOutputHelper);
            logger = xunitLoggerProvider.CreateLogger<LoggingMiddlewareTests>();
        }

        public void Dispose()
        {
            xunitLoggerProvider.Dispose();
        }

        /// <summary>
        /// Tests the constructor of <see cref="LoggingMiddleware"/> class.
        /// </summary>
        /// <param name="requestDelegate"></param>
        /// <param name="httpContextLoggingHandler"></param>
        /// <param name="httpObjectConverter"></param>
        /// <param name="localLogger"></param>
        [Theory]
        [ClassData(typeof(ConstructorTestData))]
        public void Constructor_WhenInvokedWithAtLeastOneNullParameter_MustThrowException(RequestDelegate requestDelegate
                                                                                        , IHttpContextLoggingHandler httpContextLoggingHandler
                                                                                        , IHttpObjectConverter httpObjectConverter
                                                                                        , ILogger<LoggingMiddleware> localLogger)
        {
            try
            {
                logger.LogMethodEntered();

                var exception =
                    Record.Exception(() => new LoggingMiddleware(requestDelegate
                                                                       , httpContextLoggingHandler
                                                                       , httpObjectConverter
                                                                       , localLogger));

                exception.Should()
                         .NotBeNull()
                         .And.BeAssignableTo<ArgumentNullException>();
            }
            finally
            {
                logger.LogMethodExited();
            }
        }

        /// <summary>
        /// Tests the constructor of <see cref="LoggingMiddleware"/> class.
        /// </summary>
        [Fact]
        public void Constructor_WhenInvokedWithValidParameters_MustSucceed()
        {

            try
            {
                logger.LogMethodEntered();

                var requestDelegateMock = new Mock<RequestDelegate>();
                var httpContextLoggingHandlerMock = new Mock<IHttpContextLoggingHandler>();
                var httpObjectConverterMock = new Mock<IHttpObjectConverter>();
                var loggerMock = new Mock<ILogger<LoggingMiddleware>>();

                var exception = Record.Exception(() => new LoggingMiddleware(requestDelegateMock.Object
                                                                                   , httpContextLoggingHandlerMock.Object
                                                                                   , httpObjectConverterMock.Object
                                                                                   , loggerMock.Object));

                exception.Should().BeNull();
            }
            finally
            {
                logger.LogMethodExited();
            }
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Fact]
        public async Task Invoke_UsingNoLogging_MustSucceed()
        {
            try
            {
                logger.LogMethodEntered();
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

                var exception = await Record.ExceptionAsync(() => loggingMiddleware.Invoke(new DefaultHttpContext()));
                exception.Should().BeNull();
            }
            finally
            {
                logger.LogMethodExited();
            }
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
