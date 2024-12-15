namespace Todo.Telemetry.Http
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Logs text-based only HTTP requests and responses (e.g. plain text, JSON or XML).
    /// </summary>
    public class HttpLoggingService : IHttpContextLoggingHandler, IHttpObjectConverter
    {
        private const int BufferSize = 1000;
        private static readonly string[] TextBasedHeaderNames = ["Accept", "Content-Type"];
        private static readonly string[] TextBasedHeaderValues = ["application/json", "application/xml", "text/"];
        private const string AcceptableRequestUrlPrefix = "/api/";
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="HttpLoggingService"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public HttpLoggingService(ILogger<HttpLoggingService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool ShouldLog(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "Checking whether the HTTP context {HttpContextTraceIdentifier} should be logged or not ...",
                    httpContext.TraceIdentifier);
            }

            bool result = IsTextBased(httpContext.Request);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                string willBeLoggedOutcome = result ? string.Empty : " NOT";

                logger.LogDebug("HTTP context {HttpContextTraceIdentifier} will{WillBeLoggedOutcome} be logged",
                    httpContext.TraceIdentifier, willBeLoggedOutcome);
            }

            return result;
        }

        public Task<string> ToLogMessageAsync(HttpRequest httpRequest)
        {
            ArgumentNullException.ThrowIfNull(httpRequest);

            return InternalToLogMessageAsync(httpRequest);
        }

        public Task<string> ToLogMessageAsync(HttpResponse httpResponse)
        {
            ArgumentNullException.ThrowIfNull(httpResponse);

            return InternalToLogMessageAsync(httpResponse);
        }

        private async Task<string> InternalToLogMessageAsync(HttpRequest httpRequest)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Converting HTTP request {HttpContextTraceIdentifier} ...",
                    httpRequest.HttpContext.TraceIdentifier);
            }

            StringBuilder stringBuilder = new(BufferSize);
            stringBuilder.AppendLine($"--- REQUEST {httpRequest.HttpContext.TraceIdentifier}: BEGIN ---");

            stringBuilder.AppendLine(
                $"{httpRequest.Method} {httpRequest.Path}{httpRequest.QueryString.ToUriComponent()} {httpRequest.Protocol}");

            if (httpRequest.Headers.Any())
            {
                foreach ((string key, StringValues value) in httpRequest.Headers)
                {
                    stringBuilder.AppendLine($"{key}: {value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(await httpRequest.Body.ReadAndResetAsync());
            stringBuilder.AppendLine($"--- REQUEST {httpRequest.HttpContext.TraceIdentifier}: END ---");

            string result = stringBuilder.ToString();

            return result;
        }

        private async Task<string> InternalToLogMessageAsync(HttpResponse httpResponse)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Converting HTTP response {HttpContextTraceIdentifier} ...",
                    httpResponse.HttpContext.TraceIdentifier);
            }

            StringBuilder stringBuilder = new(BufferSize);
            stringBuilder.AppendLine($"--- RESPONSE {httpResponse.HttpContext.TraceIdentifier}: BEGIN ---");

            stringBuilder.AppendLine(
                $"{httpResponse.HttpContext.Request.Protocol} {httpResponse.StatusCode} {((HttpStatusCode)httpResponse.StatusCode).ToString()}");

            if (httpResponse.Headers.Any())
            {
                foreach ((string key, StringValues value) in httpResponse.Headers)
                {
                    stringBuilder.AppendLine($"{key}: {value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(await httpResponse.Body.ReadAndResetAsync());
            stringBuilder.AppendLine($"--- RESPONSE {httpResponse.HttpContext.TraceIdentifier}: END ---");

            string result = stringBuilder.ToString();

            return result;
        }

        private static bool IsTextBased(HttpRequest httpRequest)
        {
            return Array.Exists(TextBasedHeaderNames, headerName => IsTextBased(httpRequest, headerName))
                   || httpRequest.Path.ToUriComponent().StartsWith(AcceptableRequestUrlPrefix);
        }

        private static bool IsTextBased(HttpRequest httpRequest, string headerName)
        {
            return httpRequest.Headers.TryGetValue(headerName, out StringValues headerValues)
                   && Array.Exists(TextBasedHeaderValues, acceptedHeaderValue
                       => headerValues.Any(headerValue => headerValue.StartsWith(acceptedHeaderValue)));
        }
    }
}
