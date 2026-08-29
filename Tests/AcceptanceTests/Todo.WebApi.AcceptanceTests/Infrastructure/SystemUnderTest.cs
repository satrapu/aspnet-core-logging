namespace Todo.WebApi.AcceptanceTests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Commons.Constants;

    using Polly;

    using TechTalk.SpecFlow.Infrastructure;

    public sealed class SystemUnderTest : IAsyncDisposable
    {
        private const string TodoWebApiSourcesRelativePath = "../../../../../../Sources/Todo.WebApi";

        private static readonly TimeSpan MaxWaitTime = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan RetryWaitTime = TimeSpan.FromMilliseconds(250);
        private static readonly HttpClient HttpClient = new();

        private readonly Process systemUnderTestProcess;

        private SystemUnderTest(Process systemUnderTestProcess)
        {
            this.systemUnderTestProcess = systemUnderTestProcess;
        }

        public static async Task<SystemUnderTest> StartNewAsync
        (
            int port,
            ISpecFlowOutputHelper specFlowOutputHelper,
            IDictionary<string, string> environmentVariables = null
        )
        {
            string baseUrl = $"http://localhost:{port}";
            string healthEndpoint = $"{baseUrl}/health";

            Process process = StartSystemUnderTest(baseUrl, specFlowOutputHelper, environmentVariables);
            await WaitUntilSystemUnderTestIsHealthyAsync(healthEndpoint);

            return new SystemUnderTest(process);
        }

        private static Process StartSystemUnderTest
        (
            string urls,
            ISpecFlowOutputHelper specFlowOutputHelper,
            IDictionary<string, string> environmentVariables = null
        )
        {
            StringBuilder sb = null;

            if (environmentVariables is not null)
            {
                sb = new StringBuilder(value: "--environment ");

                foreach (KeyValuePair<string, string> environmentVariable in environmentVariables)
                {
                    sb.Append($"{environmentVariable.Key}={environmentVariable.Value} ");
                }
            }

            ProcessStartInfo processStartInfo = new()
            {
                FileName = "dotnet",
                Arguments = $"""
                             run --urls="{urls}" {sb?.ToString()}
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
                await Policy.TimeoutAsync(MaxWaitTime)
                            .WrapAsync(innerPolicy: Policy.Handle<Exception>().WaitAndRetryForeverAsync(_ => RetryWaitTime))
                            .ExecuteAndCaptureAsync(() => HttpClient.GetAsync(healthEndpoint));

            if (policyResult.Outcome == OutcomeType.Failure)
            {
                throw new InvalidOperationException($"The ASP.NET Core process did not start after waiting more than {MaxWaitTime.TotalSeconds} seconds");
            }
        }

        public ValueTask DisposeAsync()
        {
            if (!systemUnderTestProcess.HasExited)
            {
                systemUnderTestProcess.Kill(entireProcessTree: true);
            }

            systemUnderTestProcess.Close();
            systemUnderTestProcess.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
