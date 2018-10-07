using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TodoWebApp.Logging;

namespace TodoWebApp.Filters
{
    /// <summary>
    /// Filter which executes after authorization filters, but before model binding filters.
    /// This ensures logging messages will not be lost in case of an invalid model.
    /// FIXME: On the other hand, they will get lost in case of an unauthenticated user!!!!
    /// </summary>
    public class LoggingFilter : IAsyncResourceFilter
    {
        private readonly ILogger logger;

        public LoggingFilter(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext resourceExecutingContext, ResourceExecutionDelegate next)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("OnResourceExecutionAsync - BEGIN");
                logger.Log(LogLevel.Debug, new EventId(7788, "Render.ResourceExecutingContext"), resourceExecutingContext, null, (executingContext, exception) => SerializeContext(executingContext));
            }

            var resourceExecutedContext = await next();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log(LogLevel.Debug, new EventId(7789, "Render.ResourceExecutedContext"), resourceExecutedContext, null, (executedContext, exception) => SerializeContext(executedContext));
                logger.LogDebug("OnResourceExecutionAsync - END");
            }
        }

        private static string SerializeContext(ResourceExecutingContext resourceExecutingContext)
        {
            var stringBuilder = new StringBuilder(1000);
            var request = resourceExecutingContext.HttpContext.Request;

            stringBuilder.AppendLine("--- REQUEST: BEGIN ---");
            stringBuilder.AppendLine($"{request.Method} {request.Path}{request.QueryString.ToUriComponent()} {request.Protocol}");

            if (request.Headers.Any())
            {
                foreach (var header in request.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {header.Value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(request.Body.ReadContentsAndReset());
            stringBuilder.AppendLine("--- REQUEST: END ---");

            return stringBuilder.ToString();
        }

        private static string SerializeContext(ResourceExecutedContext resourceExecutedContext)
        {
            var stringBuilder = new StringBuilder(1000);
            var response = resourceExecutedContext.HttpContext.Response;

            stringBuilder.AppendLine("--- RESPONSE: BEGIN ---");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"{resourceExecutedContext.HttpContext.Request.Protocol} {response.StatusCode} {((HttpStatusCode)response.StatusCode).ToString()}");

            if (response.Headers.Any())
            {
                foreach (var header in response.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {header.Value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(response.Body.ReadContentsAndReset());
            stringBuilder.AppendLine("--- RESPONSE: END ---");

            return stringBuilder.ToString();
        }
    }
}