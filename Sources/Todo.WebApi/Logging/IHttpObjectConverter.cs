using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Converts HTTP objects to strings intended to be logged using <see cref="ILogger.Log{TState}"/> method.
    /// </summary>
    public interface IHttpObjectConverter
    {
        /// <summary>
        /// Converts the given <paramref name="httpRequest"/> to a string.
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequest"/> instance to be converted.</param>
        /// <returns>The string representation of an <see cref="HttpRequest"/> instance.</returns>
        Task<string> ToLogMessageAsync(HttpRequest httpRequest);

        /// <summary>
        /// Converts the given <paramref name="httpResponse"/> to a string.
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponse"/> instance to be converted.</param>
        /// <returns>The string representation of an <see cref="HttpResponse"/> instance.</returns>
        Task<string> ToLogMessageAsync(HttpResponse httpResponse);
    }
}
