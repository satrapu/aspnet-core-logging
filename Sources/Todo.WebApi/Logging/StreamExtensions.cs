using System;
using System.IO;
using System.Text;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains extension methods applicable to <see cref="Stream"/> instances.
    /// </summary>
    public static class StreamExtensions
    {
        private const int BUFFER_SIZE = 1024;

        /// <summary>
        /// Reads the whole content of a given <see cref="Stream"/> instance and then sets its position to the beginning.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read and reset.</param>
        /// <returns>The <see cref="Stream"/> contents as a <see cref="Encoding.UTF8"/> string.</returns>
        public static string ReadAndReset(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            string result;
            stream.Seek(0, SeekOrigin.Begin);

            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, BUFFER_SIZE, true))
            {
                result = streamReader.ReadToEnd();
            }

            stream.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}
