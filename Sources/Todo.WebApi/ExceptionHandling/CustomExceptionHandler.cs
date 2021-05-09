namespace Todo.WebApi.ExceptionHandling
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Npgsql;

    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Handles any exceptions occurring inside this application.
    /// <br/>
    /// Based on https://andrewlock.net/creating-a-custom-error-handler-middleware-function/.
    /// </summary>
    public static class CustomExceptionHandler
    {
        private const string ErrorData = "errorData";
        private const string ErrorId = "errorId";
        private const string ErrorKey = "errorKey";
        private const string ProblemDetailContentType = "application/problem+json";

        /// <summary>
        /// Handles the exception occurring while processing the current HTTP request.
        /// </summary>
        /// <param name="httpContext">The HTTP context containing the unhandled exception.</param>
        public static async Task HandleException(HttpContext httpContext)
        {
            IServiceProvider serviceProvider = httpContext.RequestServices;
            IOptions<ExceptionHandlingOptions> exceptionHandlingOptions =
                serviceProvider.GetRequiredService<IOptions<ExceptionHandlingOptions>>();

            ILogger logger =
                serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(CustomExceptionHandler));

            // Try and retrieve the error from the ExceptionHandler middleware
            IExceptionHandlerFeature exceptionHandlerFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
            Exception unhandledException = exceptionHandlerFeature.Error;
            ProblemDetails problemDetails = ConvertToProblemDetails(unhandledException,
                exceptionHandlingOptions.Value.IncludeDetails);

            // The exception is first logged by the Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware class,
            // then by this method, but it's worth it since the second time the exception is logged, we end up with
            // the error ID accompanying the logged exception stack trace.
            // Whenever the web API responds with a ProblemDetails instance, the user has the opportunity to report
            // this issue and hopefully, he will mention this error ID, thus easing the job of the developer
            // assigned to fix it.
            logger.LogError(unhandledException,
                "An unexpected error with id: {ErrorId} has been caught; see more details here: {@ProblemDetails}",
                problemDetails.Extensions[ErrorId], problemDetails);

            // satrapu April 30th, 2021: Do not send exception Data dictionary over the wire since it may contain
            // sensitive data!
            problemDetails.Extensions.Remove(ErrorData);

            httpContext.Response.ContentType = ProblemDetailContentType;
            httpContext.Response.StatusCode = problemDetails.Status ?? (int) HttpStatusCode.InternalServerError;

            // Since the ProblemDetails instance is always serialized as JSON, the web API will not be able to
            // correctly handle 'Accept' HTTP header.
            // @satrapu April 1st 2020: Find a way to use ASP.NET Core content negotiation to serialize
            // the ProblemDetails instance in the format expected by the client, if possible.
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, problemDetails);
        }

        private static ProblemDetails ConvertToProblemDetails(Exception exception, bool includeDetails)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int) GetHttpStatusCode(exception),
                Title = "An unexpected error occurred while trying to process the current request",
                Detail = includeDetails ? exception?.ToString() : string.Empty,
                Extensions =
                {
                    {ErrorData, exception?.Data},
                    {ErrorId, Guid.NewGuid().ToString("N")},
                    {ErrorKey, GetErrorKey(exception)}
                }
            };

            return problemDetails;
        }

        private static HttpStatusCode GetHttpStatusCode(Exception exception)
        {
            return exception switch
            {
                EntityNotFoundException _ => HttpStatusCode.NotFound,

                // Return HTTP status code 503 in case calling the underlying database resulted in an exception.
                // See more here: https://stackoverflow.com/q/1434315.
                NpgsqlException _ => HttpStatusCode.ServiceUnavailable,

                // Also return HTTP status code 503 in case the inner exception was thrown by a call made against the
                // underlying database.
                { InnerException: NpgsqlException _ } => HttpStatusCode.ServiceUnavailable,

                // Fallback to HTTP status code 500.
                _ => HttpStatusCode.InternalServerError
            };
        }

        private static string GetErrorKey(Exception exception)
        {
            return exception switch
            {
                EntityNotFoundException _ => "entity-not-found",
                NpgsqlException _ => "database-error",
                { InnerException: NpgsqlException _ } => "database-error",

                // Fallback value
                _ => "internal-server-error"
            };
        }
    }
}
