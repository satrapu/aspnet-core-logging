using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Todo.Services;

namespace Todo.WebApi.ExceptionHandling
{
    /// <summary>
    /// Based on https://andrewlock.net/creating-a-custom-error-handler-middleware-function/.
    /// </summary>
    public static class CustomExceptionHandlerHelper
    {
        public static void UseCustomExceptionHandler(this IApplicationBuilder applicationBuilder,
            IHostEnvironment hostEnvironment)
        {
            ILogger logger = applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(CustomExceptionHandlerHelper));

            if (hostEnvironment.IsDevelopment())
            {
                applicationBuilder.Use((httpContext, _) => WriteResponse(httpContext, logger, true));
            }
            else
            {
                applicationBuilder.Use((httpContext, _) => WriteResponse(httpContext, logger, false));
            }
        }

        private static async Task WriteResponse(HttpContext httpContext, ILogger logger, bool includeDetails)
        {
            // Try and retrieve the error from the ExceptionHandler middleware
            IExceptionHandlerFeature exceptionHandlerFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
            Exception unhandledException = exceptionHandlerFeature?.Error;

            // Should always exist, but best to be safe!
            if (unhandledException == null)
            {
                return;
            }

            ProblemDetails problemDetails = ConvertToProblemDetails(unhandledException, includeDetails);

            // ProblemDetails has it's own content type
            httpContext.Response.ContentType = "application/problem+json";
            httpContext.Response.StatusCode = problemDetails.Status ?? (int) HttpStatusCode.InternalServerError;

            // The exception is first logged by the Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware class,
            // then by this method, but it's worth it since the second time the exception is logged, we end up with
            // the errorId accompanying the logged stack trace.
            // Whenever the web API responds with a ProblemDetails instance, the user has the opportunity to report
            // this issue and hopefully, he will mention the 'errorId', thus easing the job of the developer
            // assigned to fix it.
            logger.LogError(unhandledException,
                "An unhandled exception has been caught; its associated id is: {ErrorId}",
                problemDetails.Extensions["errorId"]);

            // Since the ProblemDetails instance is always serialized as JSON, the web API will not be able to
            // correctly handle 'Accept' HTTP header.
            // @satrapu April 1st 2020: Find a way to use ASP.NET Core content negotiation to serialize
            // the ProblemDetails instance in the format expected by the client.
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, problemDetails).ConfigureAwait(false);
        }

        private static ProblemDetails ConvertToProblemDetails(Exception exception, bool includeDetails)
        {
            string title = includeDetails ? $"An error occured: {exception.Message}" : "An error occured";
            string details = includeDetails ? exception.ToString() : null;

            var problemDetails = new ProblemDetails
            {
                Status = (int) ConvertToHttpStatusCode(exception),
                Title = title,
                Detail = details,
                Extensions = {{"errorId", Guid.NewGuid().ToString("N")}}
            };
            return problemDetails;
        }

        private static HttpStatusCode ConvertToHttpStatusCode(Exception exception)
        {
            return exception switch
            {
                EntityNotFoundException _ => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}