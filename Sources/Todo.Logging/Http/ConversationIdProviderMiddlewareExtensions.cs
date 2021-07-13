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
        /// <param name="applicationBuilder"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
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
