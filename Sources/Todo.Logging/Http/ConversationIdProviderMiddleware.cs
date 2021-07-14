namespace Todo.Logging.Http
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Commons.Constants;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Provides conversation IDs to each request to allow grouping them into conversations.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConversationIdProviderMiddleware
    {
        private readonly RequestDelegate nextRequestDelegate;
        private readonly ILogger logger;

        public ConversationIdProviderMiddleware(RequestDelegate nextRequestDelegate,
            ILogger<ConversationIdProviderMiddleware> logger)
        {
            this.nextRequestDelegate =
                nextRequestDelegate ?? throw new ArgumentNullException(nameof(nextRequestDelegate));

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string conversationIdKey = Logging.ConversationId;

            if (!httpContext.Request.Headers.TryGetValue(conversationIdKey, out StringValues conversationId)
                || string.IsNullOrWhiteSpace(conversationId))
            {
                conversationId = Guid.NewGuid().ToString("N");
                httpContext.Request.Headers.Add(conversationIdKey, conversationId);
            }

            httpContext.Response.Headers.Add(conversationIdKey, conversationId);

            using (logger.BeginScope(new Dictionary<string, object>
            {
                [conversationIdKey] = conversationId.ToString()
            }))
            {
                await nextRequestDelegate(httpContext);
            }
        }
    }
}
