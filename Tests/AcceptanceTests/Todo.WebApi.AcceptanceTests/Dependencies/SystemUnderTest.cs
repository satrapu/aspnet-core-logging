namespace Todo.WebApi.AcceptanceTests.Dependencies
{
    using System.Diagnostics;
    using System.IO;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Polly;
    using Polly.Retry;

    using TechTalk.SpecFlow.Infrastructure;

    public class SystemUnderTest : IAsyncDisposable
    {
        private static readonly TimeSpan MaxWaitTime = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan RetryWaitTime = TimeSpan.FromMilliseconds(500);
        private static readonly HttpClient HttpClient = new();
        private readonly Process systemUnderTest;

        private SystemUnderTest(Process systemUnderTest)
        {
            this.systemUnderTest = systemUnderTest;
        }

        public static async Task<SystemUnderTest> StartNewAsync(int port, ISpecFlowOutputHelper specFlowOutputHelper)
        {
            string baseUrl = $"http://localhost:{port}";
            string endpoint = $"{baseUrl}/health";

            Process process = StartSystemUnderTest(baseUrl, specFlowOutputHelper);
            await WaitUntilSystemUnderTestIsAvailableAsync(endpoint);

            return new SystemUnderTest(process);
        }

        private static Process StartSystemUnderTest(string urls, ISpecFlowOutputHelper specFlowOutputHelper)
        {
            DirectoryInfo todoWebApiDirectoryInfo = new("../../../../../../Sources/Todo.WebApi");

            ProcessStartInfo processStartInfo = new()
            {
                FileName = "dotnet",
                Arguments = $"run --urls=\"{urls}\" --environment AcceptanceTests",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = todoWebApiDirectoryInfo.FullName
            };

            Process process = Process.Start(processStartInfo);

            if (process is null)
            {
                throw new InvalidOperationException("Failed to start ASP.NET Core process");
            }

            process.OutputDataReceived += (_, dataReceivedEventArgs) => specFlowOutputHelper.WriteLine(dataReceivedEventArgs.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (_, dataReceivedEventArgs) => specFlowOutputHelper.WriteLine(dataReceivedEventArgs.Data);
            process.BeginErrorReadLine();

            return process;
        }

        private static async Task WaitUntilSystemUnderTestIsAvailableAsync(string endpoint)
        {
            AsyncRetryPolicy retryPolicy =
                Policy
                    .Handle<Exception>()
                    .WaitAndRetryForeverAsync(_ => RetryWaitTime);

            PolicyResult<HttpResponseMessage> result =
                await Policy
                    .TimeoutAsync(MaxWaitTime)
                    .WrapAsync(retryPolicy)
                    .ExecuteAndCaptureAsync(() => HttpClient.GetAsync(endpoint));

            if (result.Outcome == OutcomeType.Failure)
            {
                throw new InvalidOperationException($"The ASP.NET Core process did not start after waiting more than {MaxWaitTime.TotalSeconds} seconds");
            }
        }

        public ValueTask DisposeAsync()
        {
            if (!systemUnderTest.HasExited)
            {
                systemUnderTest.Kill(entireProcessTree: true);
            }

            systemUnderTest.Close();
            systemUnderTest.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
