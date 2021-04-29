namespace Todo.WebApi.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Provides conversation IDs to each request to allow grouping them into conversations.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConversationIdProviderMiddleware
    {
        private const string ConversationId = "ConversationId";
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
            if (!httpContext.Request.Headers.TryGetValue(ConversationId, out StringValues conversationId)
                || string.IsNullOrWhiteSpace(conversationId))
            {
                conversationId = Guid.NewGuid().ToString("N");
                httpContext.Request.Headers.Add(ConversationId, conversationId);
            }

            httpContext.Response.Headers.Add(ConversationId, conversationId);

            using (logger.BeginScope(new Dictionary<string, object>
            {
                [ConversationId] = conversationId.ToString()
            }))
            {
                await nextRequestDelegate(httpContext);
            }
        }
    }
}
