using System.IO;
using System.Text;

namespace TodoWebApp.Logging
{
    /// <summary>
    /// Contains extension methods applicable to <see cref="Stream"/> instances.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads the content of a given <see cref="Stream"/> instance and then resets it to the beginning.
        /// </summary>
        /// <param name="stream">THe <see cref="Stream"/> to read and reset.</param>
        /// <returns>The <see cref="Stream"/> contents as a <see cref="Encoding.UTF8"/> string.</returns>
        public static string ReadContentsAndReset(this Stream stream)
        {
            string result;
            stream.Seek(0, SeekOrigin.Begin);

            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                result = streamReader.ReadToEnd();
            }

            stream.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}
