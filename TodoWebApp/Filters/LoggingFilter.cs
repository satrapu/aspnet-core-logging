using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
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

        public LoggingFilter(ILogger<LoggingFilter> logger)
        {
            this.logger = logger;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext resourceExecutingContext, ResourceExecutionDelegate next)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("OnResourceExecutionAsync - BEGIN");
                logger.Log(LogLevel.Debug, new EventId(7788, "Render.ResourceExecutingContext"), resourceExecutingContext, null,
                    (executingContext, exception) => executingContext.ToLogMessage());
            }

            var resourceExecutedContext = await next();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log(LogLevel.Debug, new EventId(7789, "Render.ResourceExecutedContext"), resourceExecutedContext, null,
                    (executedContext, exception) => executedContext.ToLogMessage());
                logger.LogDebug("OnResourceExecutionAsync - END");
            }
        }
    }
}