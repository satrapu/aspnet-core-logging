using log4net.ObjectRenderer;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Net;
using System.Text;

namespace TodoWebApp.Logging
{
    /// <summary>
    /// An <see cref="IObjectRenderer"/> implementation which logs an <see cref="ResourceExecutedContext"/> instance
    /// to the registered appenders; in other words, it logs an HTTP response.
    /// </summary>
    public static class ResourceExecutedContextLoggingExtensions
    {
        public static string ToLogMessage(this ResourceExecutedContext resourceExecutedContext)
        {
            var stringBuilder = new StringBuilder(1000);
            var response = resourceExecutedContext.HttpContext.Response;

            stringBuilder.AppendLine("--- RESPONSE: BEGIN ---");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"{resourceExecutedContext.HttpContext.Request.Protocol} {response.StatusCode} {((HttpStatusCode)response.StatusCode).ToString()}");

            if (response.Headers.Any())
            {
                foreach (var header in response.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {header.Value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(response.Body.ReadContentsAndReset());
            stringBuilder.AppendLine("--- RESPONSE: END ---");

            return stringBuilder.ToString();
        }
    }
}