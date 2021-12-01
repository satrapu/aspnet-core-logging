namespace Todo.Logging.Http
{
    using System;

    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Contains extension methods applicable to <see cref="IApplicationBuilder"/> instances.
    /// </summary>
    public static class ConversationIdProviderMiddlewareExtensions
    {
        /// <summary>
        /// Adds middleware for providing a conversation ID to each HTTP request.
        /// </summary>
        /// <param name="applicationBuilder">Configures ASP.NET Core request processing pipeline.</param>
        /// <returns>The given <paramref name="applicationBuilder"/> instance.</returns>
        public static IApplicationBuilder UseConversationId(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            applicationBuilder.UseMiddleware<ConversationIdProviderMiddleware>();
            return applicationBuilder;
        }
    }
}
