using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text;

namespace TodoWebApp.Logging
{
    /// <summary>
    /// Contains extension methods applicable to <see cref="HttpRequest"/> instances.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Creates a ready-to-be-logged string representation of the given <paramref name="httpRequest"/>.
        /// </summary>
        /// <param name="httpRequest">An <see cref="HttpRequest"/> instance prepared for logging.</param>
        /// <returns></returns>
        public static string ToLogMessage(this HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            var stringBuilder = new StringBuilder(1000);

            stringBuilder.AppendLine($"--- REQUEST {httpRequest.HttpContext.TraceIdentifier}: BEGIN ---");
            stringBuilder.AppendLine($"{httpRequest.Method} {httpRequest.Path}{httpRequest.QueryString.ToUriComponent()} {httpRequest.Protocol}");

            if (httpRequest.Headers.Any())
            {
                foreach (var header in httpRequest.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {header.Value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(httpRequest.Body.ReadAndReset());
            stringBuilder.AppendLine($"--- REQUEST: END ---");

            return stringBuilder.ToString();
        }
    }
}