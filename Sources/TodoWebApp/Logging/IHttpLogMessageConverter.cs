using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TodoWebApp.Logging
{
    /// <summary>
    /// Converts HTTP objects to strings intended to be logged using <see cref="ILogger.Log{TState}"/> method.
    /// </summary>
    public interface IHttpLogMessageConverter
    {
        /// <summary>
        /// Converts the given <paramref name="httpRequest"/> to a string.
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequest"/> instance to be converted.</param>
        /// <returns>The string representation of an <see cref="HttpRequest"/> instance.</returns>
        string ToLogMessage(HttpRequest httpRequest);

        /// <summary>
        /// Converts the given <paramref name="httpResponse"/> to a string.
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponse"/> instance to be converted.</param>
        /// <returns>The string representation of an <see cref="HttpResponse"/> instance.</returns>
        string ToLogMessage(HttpResponse httpResponse);
    }
}
