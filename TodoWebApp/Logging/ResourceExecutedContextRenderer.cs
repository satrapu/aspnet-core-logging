using log4net.ObjectRenderer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.IO;
using System.Net;

namespace TodoWebApp.Logging
{
    /// <summary>
    /// An <see cref="IObjectRenderer"/> implementation which logs an <see cref="ResourceExecutedContext"/> instance
    /// to the registered appenders; in other words, it logs an HTTP response.
    /// </summary>
    public class ResourceExecutedContextRenderer : IObjectRenderer
    {
        public void RenderObject(RendererMap rendererMap, object objectToRender, TextWriter writer)
        {
            if (!(objectToRender is ResourceExecutedContext executedContext))
            {
                throw new InvalidOperationException($"Unable to render type: {objectToRender.GetType().FullName}; expected type is: {typeof(ResourceExecutedContext).FullName}.");
            }

            var response = executedContext.HttpContext.Response;

            writer.WriteLine("--- RESPONSE: BEGIN ---");
            writer.WriteLine($"");
            writer.WriteLine(
                $"{executedContext.HttpContext.Request.Protocol} {response.StatusCode} {((HttpStatusCode)response.StatusCode).ToString()}");

            if (response.Headers.Any())
            {
                foreach (var header in response.Headers)
                {
                    writer.WriteLine($"{header.Key}: {header.Value}");
                }
            }

            writer.WriteLine();
            writer.WriteLine(response.Body.ReadContentsAndReset());
            writer.WriteLine("--- RESPONSE: END ---");
        }
    }
}