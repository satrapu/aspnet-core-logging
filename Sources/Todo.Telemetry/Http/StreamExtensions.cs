namespace Todo.Telemetry.Http
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

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
            ArgumentNullException.ThrowIfNull(stream);

            return ReadAndResetInternalAsync(stream);
        }

        private static async Task<string> ReadAndResetInternalAsync(this Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            stream.Seek(0, SeekOrigin.Begin);

            using StreamReader streamReader = new(stream, Encoding.UTF8, true, BufferSize, true);
            string result = await streamReader.ReadToEndAsync();

            stream.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}
