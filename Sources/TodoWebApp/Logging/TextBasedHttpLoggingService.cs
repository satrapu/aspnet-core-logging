using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TodoWebApp.Logging
{
    /// <summary>
    /// Handles only text-based HTTP requests and responses (e.g. plain text, JSON or XML).
    /// </summary>
    public class TextBasedHttpLoggingService : IHttpContextLoggingHandler, IHttpLogMessageConverter
    {
        private const int REQUEST_SIZE = 1000;
        private const int RESPONSE_SIZE = 1000;

        private static readonly string[] textBasedHeaderNames = { "Accept", "Content-Type" };
        private static readonly string[] textBasedFragments = { "application/json", "application/xml", "text/" };
        private static readonly Regex textBasedRegex = new Regex(@"/api/", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private readonly ILogger logger;

        public TextBasedHttpLoggingService(ILogger<TextBasedHttpLoggingService> logger)
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

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"HTTP context {httpContext.TraceIdentifier} will be logged: {result.ToString().ToLowerInvariant()}");
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

            var stringBuilder = new StringBuilder(REQUEST_SIZE);
            stringBuilder.AppendLine($"--- REQUEST {httpRequest.HttpContext.TraceIdentifier}: BEGIN ---");
            stringBuilder.AppendLine($"{httpRequest.Method} {httpRequest.Path}{httpRequest.QueryString.ToUriComponent()} {httpRequest.Protocol}");

            if (httpRequest.Headers.Any())
            {
                foreach (var header in httpRequest.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {header.Value}");
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

            var stringBuilder = new StringBuilder(RESPONSE_SIZE);
            stringBuilder.AppendLine($"--- RESPONSE {httpResponse.HttpContext.TraceIdentifier}: BEGIN ---");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"{httpResponse.HttpContext.Request.Protocol} {httpResponse.StatusCode} {((HttpStatusCode)httpResponse.StatusCode).ToString()}");

            if (httpResponse.Headers.Any())
            {
                foreach (var header in httpResponse.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {header.Value}");
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
                || textBasedRegex.IsMatch(httpRequest.Path);
        }

        private static bool IsTextBased(HttpRequest httpRequest, string headerName)
        {
            return httpRequest.Headers.TryGetValue(headerName, out var headerValue)
                && textBasedFragments.Any(fragment => headerValue.Contains(fragment));
        }
    }
}
