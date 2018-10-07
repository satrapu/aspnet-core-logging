using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TodoWebApp.Logging;

namespace TodoWebApp.Filters
{
    /// <summary>
    /// Filter which executes after authorization filters, but before model binding filters - see more about filters here:
    /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1#filter-types.
    /// The logging messages originating from this filter will not be lost in case of an invalid model.
    /// FIXME: On the other hand, they will get lost in case of an unauthenticated or unauthorized user!!!!
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
                //logger.Log(LogLevel.Debug, new EventId(7788, "Render.ResourceExecutingContext"), resourceExecutingContext, null,
                //    (executingContext, exception) => executingContext.ToLogMessage());
                logger.LogDebug(resourceExecutingContext.ToLogMessage());
            }

            var resourceExecutedContext = await next();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                //logger.Log(LogLevel.Debug, new EventId(7789, "Render.ResourceExecutedContext"), resourceExecutedContext, null,
                //    (executedContext, exception) => executedContext.ToLogMessage());
                logger.LogDebug(resourceExecutedContext.ToLogMessage());
                logger.LogDebug("OnResourceExecutionAsync - END");
            }
        }
    }
}