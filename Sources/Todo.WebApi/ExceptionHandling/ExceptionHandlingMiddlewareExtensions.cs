using System;
using Microsoft.AspNetCore.Builder;

namespace Todo.WebApi.ExceptionHandling
{
    /// <summary>
    /// Contains extension methods applicable to <see cref="IApplicationBuilder"/> instances.
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Adds middleware for handling any exception thrown when invoking this web API.
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            applicationBuilder.UseMiddleware<ExceptionHandlingMiddleware>();
            return applicationBuilder;
        }
    }
}