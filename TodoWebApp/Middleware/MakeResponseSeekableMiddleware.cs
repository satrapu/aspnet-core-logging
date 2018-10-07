using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace TodoWebApp.Middleware
{
    /// <summary>
    /// Middleware used for transforming the current ASP.NET Core HTTP response in a seekable stream,
    /// thus allowing other middleware components read it and reset it.
    /// </summary>
    public class MakeResponseSeekableMiddleware
    {
        private const int BUFFER_SIZE = 2048;
        private readonly RequestDelegate nextRequestDelegate;

        /// <summary>
        /// Creates a new instance of the <see cref="MakeResponseSeekableMiddleware"/> class.
        /// </summary>
        /// <param name="nextRequestDelegate"></param>
        public MakeResponseSeekableMiddleware(RequestDelegate nextRequestDelegate)
        {
            this.nextRequestDelegate = nextRequestDelegate;
        }

        /// <summary>
        /// Transforms the current ASP.NET Core HTTP response in a seekable stream.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var originalResponseBodyStream = context.Response.Body;

            using (var memoryStream = new MemoryStream(BUFFER_SIZE * 2))
            {
                context.Response.Body = memoryStream;
                await nextRequestDelegate(context);
                await memoryStream.CopyToAsync(originalResponseBodyStream, BUFFER_SIZE);
            }
        }
    }
}