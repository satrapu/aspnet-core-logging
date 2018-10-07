using log4net.ObjectRenderer;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO;
using System.Linq;

namespace TodoWebApp.Logging
{
    /// <summary>
    /// An <see cref="IObjectRenderer"/> implementation which logs an <see cref="ResourceExecutingContext"/> instance
    /// to the registered appenders; in other words, it logs an HTTP request.
    /// </summary>
    public class ResourceExecutingContextRenderer : IObjectRenderer
    {
        public void RenderObject(RendererMap rendererMap, object objectToRender, TextWriter writer)
        {
            if (!(objectToRender is ResourceExecutingContext executingContext))
            {
                throw new InvalidOperationException($"Unable to render type: {objectToRender.GetType().FullName}; expected type is: {typeof(ResourceExecutingContext).FullName}.");
            }

            var request = executingContext.HttpContext.Request;

            writer.WriteLine("--- REQUEST: BEGIN ---");
            writer.WriteLine($"{request.Method} {request.Path}{request.QueryString.ToUriComponent()} {request.Protocol}");

            if (request.Headers.Any())
            {
                foreach (var header in request.Headers)
                {
                    writer.WriteLine($"{header.Key}: {header.Value}");
                }
            }

            writer.WriteLine();
            writer.WriteLine(request.Body.ReadContentsAndReset());
            writer.WriteLine("--- REQUEST: END ---");
        }
    }
}