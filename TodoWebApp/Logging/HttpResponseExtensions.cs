using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace TodoWebApp.Logging
{
    /// <summary>
    /// Contains extension methods applicable to <see cref="HttpResponse"/> instances.
    /// </summary>
    public static class HttpResponseExtensions
    {
        /// <summary>
        /// Creates a ready-to-be-logged string representation of the given <paramref name="httpResponse"/>.
        /// </summary>
        /// <param name="httpResponse">An <see cref="HttpResponse"/> instance prepared for logging.</param>
        /// <returns></returns>
        public static string ToLogMessage(this HttpResponse httpResponse)
        {
            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            var stringBuilder = new StringBuilder(1000);

            stringBuilder.AppendLine($"--- RESPONSE {httpResponse.HttpContext.TraceIdentifier}: BEGIN ---");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"{httpResponse.HttpContext.Request.Protocol} {httpResponse.StatusCode} {((HttpStatusCode)httpResponse.StatusCode).ToString()}");

            if (httpResponse.Headers.Any())
            {
                foreach (var header in httpResponse.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {header.Value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(httpResponse.Body.ReadAndReset());
            stringBuilder.AppendLine("--- RESPONSE: END ---");

            return stringBuilder.ToString();
        }
    }
}