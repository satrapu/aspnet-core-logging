namespace Todo.WebApi.ExceptionHandling
{
    using System;
    using System.Net;
    using System.Transactions;

    using Npgsql;

    using Services.TodoItemManagement;

    /// <summary>
    /// Maps <see cref="Exception"/> instances to <see cref="ExceptionMappingResult"/> instances.
    /// </summary>
    public static class ExceptionMappingResults
    {
        public static readonly ExceptionMappingResult EntityNotFound =
            new(HttpStatusCode.NotFound, "entity-not-found");

        public static readonly ExceptionMappingResult DatabaseError =
            new(HttpStatusCode.ServiceUnavailable, "database-error");

        public static readonly ExceptionMappingResult GenericError =
            new(HttpStatusCode.InternalServerError, "internal-server-error");

        /// <summary>
        /// Maps the given <paramref name="exception"/> to an <see cref="ExceptionMappingResult"/> instance.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> instance to map.</param>
        /// <returns>An <see cref="ExceptionMappingResult"/> instance.</returns>
        public static ExceptionMappingResult GetMappingResult(Exception exception)
        {
            return exception switch
            {
                EntityNotFoundException _ => EntityNotFound,

                // Return HTTP status code 503 in case calling the underlying database resulted in an exception.
                // See more here: https://stackoverflow.com/q/1434315.
                NpgsqlException _ => DatabaseError,

                // Also return HTTP status code 503 in case the inner exception was thrown by a call made against the
                // underlying database.
                { InnerException: NpgsqlException _ } => DatabaseError,

                TransactionException _ => DatabaseError,

                // Fall-back to HTTP status code 500.
                _ => GenericError
            };
        }
    }
}
