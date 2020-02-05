using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains extension methods applicable to <see cref="Stream"/> instances.
    /// </summary>
    public static class StreamExtensions
    {
        private const int BufferSize = 1024;

        /// <summary>
        /// Reads the whole content of a given <see cref="Stream"/> instance and then sets its position to the beginning.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read and reset.</param>
        /// <returns>The <see cref="Stream"/> contents as a <see cref="Encoding.UTF8"/> string.</returns>
        public static Task<string> ReadAndResetAsync(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return ReadAndResetInternalAsync(stream);
        }

        private static async Task<string> ReadAndResetInternalAsync(this Stream stream)
        {
            string result;
            stream.Seek(0, SeekOrigin.Begin);

            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, BufferSize, true))
            {
                result = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }

            stream.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}