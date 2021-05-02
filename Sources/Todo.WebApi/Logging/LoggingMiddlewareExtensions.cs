namespace Todo.WebApi.Logging
{
    using System;

    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Contains extension methods applicable to <see cref="IApplicationBuilder"/> instances.
    /// </summary>
    public static class LoggingMiddlewareExtensions
    {
        /// <summary>
        /// Adds middleware for logging the current <see cref="Microsoft.AspNetCore.Http.HttpContext"/> object.
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IApplicationBuilder UseHttpLogging(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            applicationBuilder.UseMiddleware<LoggingMiddleware>();
            return applicationBuilder;
        }
    }
}
