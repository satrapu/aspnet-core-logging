using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Logs text-based only HTTP requests and responses (e.g. plain text, JSON or XML).
    /// </summary>
    public class LoggingService : IHttpContextLoggingHandler, IHttpObjectConverter
    {
        private const int BUFFER_SIZE = 1000;

        private static readonly string[] textBasedHeaderNames = { "Accept", "Content-Type" };
        private static readonly string[] textBasedHeaderValues = { "application/json", "application/xml", "text/" };
        private const string ACCEPTABLE_REQUEST_URL_PREFIX = "/api/";
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="LoggingService"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public LoggingService(ILogger<LoggingService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool ShouldLog(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Checking whether the HTTP context {httpContext.TraceIdentifier} should be logged or not ...");
            }

            var result = IsTextBased(httpContext.Request);
            var willBeLoggedOutcome = result ? string.Empty : " NOT";

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"HTTP context {httpContext.TraceIdentifier} will{willBeLoggedOutcome} be logged");
            }

            return result;
        }

        public string ToLogMessage(HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Converting HTTP request {httpRequest.HttpContext.TraceIdentifier} ...");
            }

            var stringBuilder = new StringBuilder(BUFFER_SIZE);
            stringBuilder.AppendLine($"--- REQUEST {httpRequest.HttpContext.TraceIdentifier}: BEGIN ---");
            stringBuilder.AppendLine($"{httpRequest.Method} {httpRequest.Path}{httpRequest.QueryString.ToUriComponent()} {httpRequest.Protocol}");

            if (httpRequest.Headers.Any())
            {
                foreach (var (key, value) in httpRequest.Headers)
                {
                    stringBuilder.AppendLine($"{key}: {value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(httpRequest.Body.ReadAndReset());
            stringBuilder.AppendLine($"--- REQUEST {httpRequest.HttpContext.TraceIdentifier}: END ---");

            var result = stringBuilder.ToString();
            return result;
        }

        public string ToLogMessage(HttpResponse httpResponse)
        {
            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Converting HTTP response {httpResponse.HttpContext.TraceIdentifier} ...");
            }

            var stringBuilder = new StringBuilder(BUFFER_SIZE);
            stringBuilder.AppendLine($"--- RESPONSE {httpResponse.HttpContext.TraceIdentifier}: BEGIN ---");
            stringBuilder.AppendLine($"{httpResponse.HttpContext.Request.Protocol} {httpResponse.StatusCode} {((HttpStatusCode)httpResponse.StatusCode).ToString()}");

            if (httpResponse.Headers.Any())
            {
                foreach (var (key, value) in httpResponse.Headers)
                {
                    stringBuilder.AppendLine($"{key}: {value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(httpResponse.Body.ReadAndReset());
            stringBuilder.AppendLine($"--- RESPONSE {httpResponse.HttpContext.TraceIdentifier}: END ---");

            var result = stringBuilder.ToString();
            return result;
        }

        private static bool IsTextBased(HttpRequest httpRequest)
        {
            return textBasedHeaderNames.Any(headerName => IsTextBased(httpRequest, headerName))
                || httpRequest.Path.ToUriComponent().StartsWith(ACCEPTABLE_REQUEST_URL_PREFIX);
        }

        private static bool IsTextBased(HttpRequest httpRequest, string headerName)
        {
            return httpRequest.Headers.TryGetValue(headerName, out var headerValues)
                && textBasedHeaderValues.Any(acceptedHeaderValue => headerValues.Any(headerValue => headerValue.StartsWith(acceptedHeaderValue)));
        }
    }
}
