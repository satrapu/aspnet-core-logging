using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Logs HTTP requests and responses.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LoggingMiddleware
    {
        /// <summary>
        /// Represents the response buffer size.
        /// </summary>
        private const int BUFFER_SIZE_IN_BYTES = 1024 * 1024;

        private readonly RequestDelegate nextRequestDelegate;
        private readonly IHttpContextLoggingHandler httpContextLoggingHandler;
        private readonly IHttpObjectConverter httpObjectConverter;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="LoggingMiddleware"/> class.
        /// </summary>
        /// <param name="nextRequestDelegate"></param>
        /// <param name="httpContextLoggingHandler"></param>
        /// <param name="httpObjectConverter"></param>
        /// <param name="logger"></param>
        public LoggingMiddleware(RequestDelegate nextRequestDelegate
                               , IHttpContextLoggingHandler httpContextLoggingHandler
                               , IHttpObjectConverter httpObjectConverter
                               , ILogger<LoggingMiddleware> logger)
        {
            this.nextRequestDelegate = nextRequestDelegate ?? throw new ArgumentNullException(nameof(nextRequestDelegate));
            this.httpContextLoggingHandler = httpContextLoggingHandler ?? throw new ArgumentNullException(nameof(httpContextLoggingHandler));
            this.httpObjectConverter = httpObjectConverter ?? throw new ArgumentNullException(nameof(httpObjectConverter));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes the given <paramref name="httpContext"/> object.
        /// </summary>
        /// <param name="httpContext">The current HTTP context to be processed.</param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContextLoggingHandler.ShouldLog(httpContext))
            {
                await Log(httpContext);
            }
            else
            {
                await nextRequestDelegate(httpContext);
            }
        }

        /// <summary>
        /// Logs the <see cref="HttpContext.Request"/> and <see cref="HttpContext.Response"/> properties of the given <paramref name="httpContext"/> object.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> object to be logged.</param>
        /// <returns></returns>
        private async Task Log(HttpContext httpContext)
        {
            // Ensure the current HTTP request is seekable and thus can be read and reset many times, including for logging purposes
            httpContext.Request.EnableRewind();

            // Logs the current HTTP request
            var httpRequestAsLogMessage = httpObjectConverter.ToLogMessage(httpContext.Request);
            logger.LogDebug(httpRequestAsLogMessage);

            // Saves the original response body stream for latter purposes
            var originalResponseBodyStream = httpContext.Response.Body;

            using (var stream = new MemoryStream(BUFFER_SIZE_IN_BYTES))
            {
                // Replace response body stream with a seekable one, like a MemoryStream, to allow logging it
                httpContext.Response.Body = stream;

                // Process current request
                await nextRequestDelegate(httpContext);

                // Logs the current HTTP response
                var httpResponseAsLogMessage = httpObjectConverter.ToLogMessage(httpContext.Response);
                logger.LogDebug(httpResponseAsLogMessage);

                // Ensure the original HTTP response is sent to the next middleware
                await stream.CopyToAsync(originalResponseBodyStream);
            }
        }
    }
}