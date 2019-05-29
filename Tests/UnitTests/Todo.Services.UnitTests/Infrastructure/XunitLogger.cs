using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace Todo.Services.Infrastructure
{
    /// <summary>
    /// https://stackoverflow.com/a/46172875
    /// </summary>
    public class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly string categoryName;

        public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName)
        {
            this.testOutputHelper = testOutputHelper;
            this.categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel
                              , EventId eventId
                              , TState state
                              , Exception exception
                              , Func<TState, Exception, string> formatter)
        {
            testOutputHelper.WriteLine($"{categoryName} [{eventId}] {formatter(state, exception)}");

            if (exception != null)
            {
                testOutputHelper.WriteLine(exception.ToString());
            }
        }

        private class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}
