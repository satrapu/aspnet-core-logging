using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TodoWebApp.Logging
{
    public class LoggingMiddleware
    {
        private const int RESPONSE_BUFFER_SIZE_IN_BYTES = 1024 * 1024;
        private static readonly string[] textBasedHeaderNames = { "Accept", "Content-Type" };
        private static readonly string[] textBasedFragments = { "application/json", "application/xml", "text/" };
        private static readonly Regex textBasedRegex = new Regex(@"/api/", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private readonly RequestDelegate nextRequestDelegate;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="LoggingMiddleware"/> class.
        /// </summary>
        /// <param name="nextRequestDelegate"></param>
        /// <param name="logger"></param>
        public LoggingMiddleware(RequestDelegate nextRequestDelegate, ILogger<LoggingMiddleware> logger)
        {
            this.nextRequestDelegate = nextRequestDelegate ?? throw new ArgumentNullException(nameof(nextRequestDelegate));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Tries to log the current HTTP request and its response.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (IsTextBased(context.Request) && logger.IsEnabled(LogLevel.Debug))
            {
                // Ensure request is seekable and thus can be read and reset many times, including for logging purposes
                context.Request.EnableRewind();
                logger.LogDebug(context.Request.ToLogMessage());

                // Replace response body stream with a seekable one, like a MemoryStream, to allow logging it
                var originalResponseBodyStream = context.Response.Body;

                using (var memoryStream = new MemoryStream(RESPONSE_BUFFER_SIZE_IN_BYTES))
                {
                    context.Response.Body = memoryStream;
                    await nextRequestDelegate(context);
                    logger.LogDebug(context.Response.ToLogMessage());
                    await memoryStream.CopyToAsync(originalResponseBodyStream);
                }
            }
            else
            {
                await nextRequestDelegate(context);
            }
        }

        private static bool IsTextBased(HttpRequest httpRequest)
        {
            return textBasedHeaderNames.Any(headerName => IsTextBased(httpRequest, headerName))
                   || textBasedRegex.IsMatch(httpRequest.Path);
        }

        private static bool IsTextBased(HttpRequest httpRequest, string headerName)
        {
            return httpRequest.Headers.TryGetValue(headerName, out var headerValue)
                   && textBasedFragments.Any(fragment => headerValue.Contains(fragment));
        }
    }
}