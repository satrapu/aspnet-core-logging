using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Todo.WebApi.ExceptionHandling
{
    /// <summary>
    /// Handles any unhandled exception thrown when calling this web API.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate nextRequestDelegate;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="nextRequestDelegate"></param>
        /// <param name="logger"></param>
        public ExceptionHandlingMiddleware(RequestDelegate nextRequestDelegate,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            this.nextRequestDelegate =
                nextRequestDelegate ?? throw new ArgumentNullException(nameof(nextRequestDelegate));
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
            try
            {
                await nextRequestDelegate(httpContext).ConfigureAwait(false);
            }
            catch (Exception unhandledException)
            {
                Handle(unhandledException, httpContext);
            }
        }

        private void Handle(Exception unhandledException, HttpContext httpContext)
        {
            string errorId = Guid.NewGuid().ToString("N");
            logger.LogError(unhandledException,
                "An unhandled exception has been caught; associated error id is: {ErrorId}", errorId);

            httpContext.Response.Body = new MemoryStream();
            httpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            httpContext.Response.Headers.Add("ErrorId", errorId);
        }
    }
}