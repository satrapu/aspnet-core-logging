using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Todo.Services.Infrastructure
{
    /// <summary>
    /// https://stackoverflow.com/a/46172875
    /// </summary>
    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper testOutputHelper;

        public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(testOutputHelper, categoryName);
        }

        public ILogger CreateLogger<T>() where T: class
        {
            return new XunitLogger(testOutputHelper, typeof(T).FullName);
        }

        public void Dispose()
        {
        }
    }
}
