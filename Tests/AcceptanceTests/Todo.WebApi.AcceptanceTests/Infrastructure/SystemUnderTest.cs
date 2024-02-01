namespace Todo.WebApi.AcceptanceTests.Infrastructure
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Polly;

    using TechTalk.SpecFlow.Infrastructure;

    public class SystemUnderTest : IAsyncDisposable
    {
        private const string EnvironmentName = "AcceptanceTests";
        private const string TodoWebApiSourcesRelativePath = "../../../../../../Sources/Todo.WebApi";

        private static readonly TimeSpan MaxWaitTime = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan RetryWaitTime = TimeSpan.FromMilliseconds(250);
        private static readonly HttpClient HttpClient = new();

        private readonly Process systemUnderTest;

        private SystemUnderTest(Process systemUnderTest)
        {
            this.systemUnderTest = systemUnderTest;
        }

        public static async Task<SystemUnderTest> StartNewAsync(int port, ISpecFlowOutputHelper specFlowOutputHelper)
        {
            string baseUrl = $"http://localhost:{port}";
            string healthEndpoint = $"{baseUrl}/health";

            Process process = StartSystemUnderTest(baseUrl, specFlowOutputHelper);
            await WaitUntilSystemUnderTestIsHealthyAsync(healthEndpoint);

            return new SystemUnderTest(process);
        }

        private static Process StartSystemUnderTest(string urls, ISpecFlowOutputHelper specFlowOutputHelper)
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = "dotnet",
                Arguments = $"""
                             run --urls="{urls}" --environment="{EnvironmentName}"
                             """,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = new DirectoryInfo(path: TodoWebApiSourcesRelativePath).FullName
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

        private static async Task WaitUntilSystemUnderTestIsHealthyAsync(string healthEndpoint)
        {
            PolicyResult<HttpResponseMessage> policyResult =
                await Policy
                    .TimeoutAsync(MaxWaitTime)
                    .WrapAsync(innerPolicy: Policy.Handle<Exception>().WaitAndRetryForeverAsync(_ => RetryWaitTime))
                    .ExecuteAndCaptureAsync(() => HttpClient.GetAsync(healthEndpoint));

            if (policyResult.Outcome == OutcomeType.Failure)
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
