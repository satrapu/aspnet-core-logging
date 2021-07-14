namespace Todo.WebApi.TestInfrastructure
{
    using System.Net.Http;
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

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("-- REQUEST: BEGIN --");
            stringBuilder.AppendLine($"{request}\nContent:\n\t");

            if (request.Content != null)
            {
                stringBuilder.AppendLine(await request.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                stringBuilder.AppendLine("N/A");
            }

            stringBuilder.AppendLine("-- REQUEST: END --");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            stringBuilder.AppendLine("-- RESPONSE: BEGIN --");
            stringBuilder.AppendLine($"{response}\nContent:\n\t");

            if (request.Content != null)
            {
                stringBuilder.AppendLine(await response.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                stringBuilder.AppendLine("N/A");
            }

            stringBuilder.AppendLine("-- RESPONSE: END --");

            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            logger.LogInformation(stringBuilder.ToString());

            return response;
        }
    }
}
