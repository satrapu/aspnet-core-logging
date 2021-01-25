using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Todo.WebApi.Infrastructure
{
    /// <summary>
    /// Contains extensions methods applicable to <see cref="HttpResponseMessage"/> instances.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Logs the given <paramref name="response"/> to console.
        /// </summary>
        /// <param name="response">The HTTP response to log.</param>
        /// <param name="description">The description to include inside the log message to ease
        /// identifying the HTTP response.</param>
        public static void LogToConsole(this HttpResponseMessage response, string description)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var stringBuilder = new StringBuilder();
            string responseType;
            TextWriter textWriter;

            if (response.IsSuccessStatusCode)
            {
                responseType = "Successful";
                textWriter = Console.Out;
            }
            else
            {
                responseType = "Unsuccessful";
                textWriter = Console.Error;
            }

            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                responseContent = "N/A";
            }

            stringBuilder.AppendLine($"{responseType} response: ({description})");
            stringBuilder.AppendLine($"\tStatus: {(int) response.StatusCode} {response.ReasonPhrase}");
            stringBuilder.AppendLine($"\tURI: {response.RequestMessage.RequestUri.AbsoluteUri}");
            stringBuilder.AppendLine($"\tHTTP method: {response.RequestMessage.Method}");
            stringBuilder.AppendLine($"\tContent: {responseContent}");

            textWriter.WriteLine(stringBuilder.ToString());
        }
    }
}