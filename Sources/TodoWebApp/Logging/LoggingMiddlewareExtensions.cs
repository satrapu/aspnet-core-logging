using Microsoft.AspNetCore.Builder;

namespace TodoWebApp.Logging
{
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpLogging(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<LoggingMiddleware>();
            return applicationBuilder;
        }
    }
}
