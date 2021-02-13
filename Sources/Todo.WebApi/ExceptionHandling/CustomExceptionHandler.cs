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
using Npgsql;
using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.WebApi.ExceptionHandling
{
    /// <summary>
    /// Contains extension methods applicable to <see cref="IApplicationBuilder"/> instances
    /// for handling exceptions thrown by this web API.
    /// <br/>
    /// Based on https://andrewlock.net/creating-a-custom-error-handler-middleware-function/.
    /// </summary>
    public static class CustomExceptionHandler
    {
        private const string ErrorId = "errorId";
        private const string ProblemDetailContentType = "application/problem+json";

        /// <summary>
        /// Configures handling the exceptions thrown by any part of this web API.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/> instance which needs to be
        /// configured to handle exceptions.</param>
        /// <param name="hostEnvironment">The <see cref="IHostEnvironment"/> instance inside which this
        /// web API runs.</param>
        public static void UseCustomExceptionHandler(this IApplicationBuilder applicationBuilder,
            IHostEnvironment hostEnvironment)
        {
            ILogger logger = applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(CustomExceptionHandler));
            bool includeDetails = !hostEnvironment.IsStaging() && !hostEnvironment.IsProduction();
            applicationBuilder.Use((httpContext, _) => WriteResponse(httpContext, logger, includeDetails));
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
            httpContext.Response.ContentType = ProblemDetailContentType;
            httpContext.Response.StatusCode = problemDetails.Status ?? (int) HttpStatusCode.InternalServerError;

            // The exception is first logged by the Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware class,
            // then by this method, but it's worth it since the second time the exception is logged, we end up with
            // the errorId accompanying the logged stack trace.
            // Whenever the web API responds with a ProblemDetails instance, the user has the opportunity to report
            // this issue and hopefully, he will mention the 'errorId', thus easing the job of the developer
            // assigned to fix it.
            logger.LogError(unhandledException,
                "An unhandled exception has been caught; its associated id is: {ErrorId}",
                problemDetails.Extensions[ErrorId]);

            // Since the ProblemDetails instance is always serialized as JSON, the web API will not be able to
            // correctly handle 'Accept' HTTP header.
            // @satrapu April 1st 2020: Find a way to use ASP.NET Core content negotiation to serialize
            // the ProblemDetails instance in the format expected by the client.
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, problemDetails);
        }

        private static ProblemDetails ConvertToProblemDetails(Exception exception, bool includeDetails)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int) ConvertToHttpStatusCode(exception),
                Title = "An error occured while trying to process the current request",
                Detail = includeDetails ? exception.ToString() : string.Empty,
                Extensions = {{ErrorId, Guid.NewGuid().ToString("N")}}
            };
            return problemDetails;
        }

        private static HttpStatusCode ConvertToHttpStatusCode(Exception exception)
        {
            return exception switch
            {
                EntityNotFoundException _ => HttpStatusCode.NotFound,

                // Return HTTP status code 503 in case calling the underlying database resulted in an exception.
                // See more here: https://stackoverflow.com/q/1434315.
                NpgsqlException _ => HttpStatusCode.ServiceUnavailable,

                // Also return HTTP status code 503 in case the inner exception was thrown by a call made against the
                // underlying database.
                {InnerException: NpgsqlException _} => HttpStatusCode.ServiceUnavailable,

                // Fallback to HTTP status code 500.
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}