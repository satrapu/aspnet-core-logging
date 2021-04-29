namespace Todo.WebApi.Logging
{
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Handles the logic of logging <see cref="HttpContext"/> instances.
    /// </summary>
    public interface IHttpContextLoggingHandler
    {
        /// <summary>
        /// Checks whether the given <paramref name="httpContext"/> should be logged or not.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        bool ShouldLog(HttpContext httpContext);
    }
}
