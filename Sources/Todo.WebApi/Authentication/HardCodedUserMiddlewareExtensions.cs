using Microsoft.AspNetCore.Builder;
using System;

namespace Todo.WebApi.Authentication
{
    /// <summary>
    /// Contains extension methods applicable to <see cref="IApplicationBuilder"/> instances.
    /// </summary>
    public static class HardCodedUserMiddlewareExtensions
    {
        /// <summary>
        /// Adds middleware for setting the current user to a hard-coded value.
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IApplicationBuilder UseHardCodedUser(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            applicationBuilder.UseMiddleware<HardCodedUserMiddleware>();
            return applicationBuilder;
        }
    }
}
