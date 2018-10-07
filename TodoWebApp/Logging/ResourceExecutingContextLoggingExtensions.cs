using log4net.ObjectRenderer;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Text;

namespace TodoWebApp.Logging
{
    /// <summary>
    /// An <see cref="IObjectRenderer"/> implementation which logs an <see cref="ResourceExecutingContext"/> instance
    /// to the registered appenders; in other words, it logs an HTTP request.
    /// </summary>
    public static class ResourceExecutingContextLoggingExtensions
    {
        public static string ToLogMessage(this ResourceExecutingContext resourceExecutingContext)
        {
            var stringBuilder = new StringBuilder(1000);
            var request = resourceExecutingContext.HttpContext.Request;

            stringBuilder.AppendLine("--- REQUEST: BEGIN ---");
            stringBuilder.AppendLine($"{request.Method} {request.Path}{request.QueryString.ToUriComponent()} {request.Protocol}");

            if (request.Headers.Any())
            {
                foreach (var header in request.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {header.Value}");
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(request.Body.ReadContentsAndReset());
            stringBuilder.AppendLine("--- REQUEST: END ---");

            return stringBuilder.ToString();
        }
    }
}