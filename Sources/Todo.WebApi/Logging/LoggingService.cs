using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Logs text-based only HTTP requests and responses (e.g. plain text, JSON or XML).
    /// </summary>
    public class LoggingService : IHttpContextLoggingHandler, IHttpObjectConverter
    {
        private const int BufferSize = 1000;

        private static readonly string[] TextBasedHeaderNames = { "Accept", "Content-Type" };
        private static readonly string[] TextBasedHeaderValues = { "application/json", "application/xml", "text/" };
        private const string AcceptableRequestUrlPrefix = "/api/";
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
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                logger.LogDebug($"Checking whether the HTTP context {httpContext.TraceIdentifier} should be logged or not ...");
            }

            var result = IsTextBased(httpContext.Request);
            var willBeLoggedOutcome = result ? string.Empty : " NOT";

            if (logger.IsEnabled(LogLevel.Debug))
            {
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                logger.LogDebug($"HTTP context {httpContext.TraceIdentifier} will{willBeLoggedOutcome} be logged");
            }

            return result;
        }

        public Task<string> ToLogMessageAsync(HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            return InternalToLogMessageAsync(httpRequest);
        }

        public Task<string> ToLogMessageAsync(HttpResponse httpResponse)
        {
            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            return InternalToLogMessageAsync(httpResponse);
        }

        private async Task<string> InternalToLogMessageAsync(HttpRequest httpRequest)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                logger.LogDebug($"Converting HTTP request {httpRequest.HttpContext.TraceIdentifier} ...");
            }

            var stringBuilder = new StringBuilder(BufferSize);
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
            stringBuilder.AppendLine(await httpRequest.Body.ReadAndResetAsync().ConfigureAwait(false));
            stringBuilder.AppendLine($"--- REQUEST {httpRequest.HttpContext.TraceIdentifier}: END ---");

            var result = stringBuilder.ToString();
            return result;
        }

        private async Task<string> InternalToLogMessageAsync(HttpResponse httpResponse)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                logger.LogDebug($"Converting HTTP response {httpResponse.HttpContext.TraceIdentifier} ...");
            }

            var stringBuilder = new StringBuilder(BufferSize);
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
            stringBuilder.AppendLine(await httpResponse.Body.ReadAndResetAsync().ConfigureAwait(false));
            stringBuilder.AppendLine($"--- RESPONSE {httpResponse.HttpContext.TraceIdentifier}: END ---");

            var result = stringBuilder.ToString();
            return result;
        }

        private static bool IsTextBased(HttpRequest httpRequest)
        {
            return TextBasedHeaderNames.Any(headerName => IsTextBased(httpRequest, headerName))
                || httpRequest.Path.ToUriComponent().StartsWith(AcceptableRequestUrlPrefix);
        }

        private static bool IsTextBased(HttpRequest httpRequest, string headerName)
        {
            return httpRequest.Headers.TryGetValue(headerName, out var headerValues)
                && TextBasedHeaderValues.Any(acceptedHeaderValue => headerValues.Any(headerValue => headerValue.StartsWith(acceptedHeaderValue)));
        }
    }
}
