namespace Todo.WebApi.TestInfrastructure
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation which logs all HTTP requests and responses
    /// it handles.
    /// </summary>
    public class HttpLoggingHandler : DelegatingHandler
    {
        private readonly ILogger logger;

        public HttpLoggingHandler(HttpMessageHandler innerHandler, ILogger logger) : base(innerHandler)
        {
            this.logger = logger;
        }

        /// <summary>
        /// This method logs the HTTP request and its corresponding response using the format documented here:
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Messages.
        /// </summary>
        /// <param name="request">The HTTP request to process.</param>
        /// <param name="cancellationToken">The token used for canceling processing the given HTTP request.</param>
        /// <returns>The HTTP response received from the server.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var stringBuilder = new StringBuilder();
            await stringBuilder.AppendAsync(request, cancellationToken);
            stringBuilder.AppendLine();
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            await stringBuilder.AppendAsync(response, cancellationToken);

            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            logger.LogInformation(stringBuilder.ToString());

            return response;
        }
    }

    internal static class StringBuilderExtensions
    {
        public static async Task AppendAsync(this StringBuilder stringBuilder, HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            stringBuilder.AppendLine("-- REQUEST: BEGIN --");
            stringBuilder.AppendLine($"{request.Method.Method} {request.RequestUri.LocalPath} HTTP/{request.Version}");
            stringBuilder.AppendHeaders(request.Headers);
            await stringBuilder.AppendAsync(request.Content, cancellationToken);
            stringBuilder.AppendLine("-- REQUEST: END --");
        }

        public static async Task AppendAsync(this StringBuilder stringBuilder, HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            stringBuilder.AppendLine("-- RESPONSE: BEGIN --");
            stringBuilder.AppendLine($"HTTP/{response.Version} {(int) response.StatusCode} {response.StatusCode}");
            stringBuilder.AppendHeaders(response.Headers);
            await stringBuilder.AppendAsync(response.Content, cancellationToken);
            stringBuilder.AppendLine("-- RESPONSE: END --");
        }

        private static void AppendHeaders(this StringBuilder stringBuilder, HttpHeaders headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var header in headers)
            {
                stringBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        private static async Task AppendAsync(this StringBuilder stringBuilder, HttpContent content,
            CancellationToken cancellationToken)
        {
            if (content == null)
            {
                return;
            }

            string contentAsString = await content.ReadAsStringAsync(cancellationToken);
            stringBuilder.AppendLine($"\n{contentAsString}");
        }
    }
}
