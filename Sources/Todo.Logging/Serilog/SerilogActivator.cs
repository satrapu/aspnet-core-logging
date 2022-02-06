namespace Todo.Logging.Serilog
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using global::Serilog;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Todo.Commons.Constants;

    /// <summary>
    /// Contains extension methods used for integration Serilog with this application.
    /// </summary>
    public static class SerilogActivator
    {
        /// <summary>
        /// Adds Serilog logging provider to the current <paramref name="services"/> instance.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The given <paramref name="services"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="services"/>
        /// or <paramref name="configuration"/> is null.</exception>
        public static IServiceCollection ActivateSerilog(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddLogging(loggingBuilder =>
            {
                SetFileSinkDestinationFolder(configuration);

                loggingBuilder
                    .ClearProviders()
                    // Ensure events produces by ILogger will be exported by Open Telemetry to Jaeger.
                    // See more here: https://github.com/open-telemetry/opentelemetry-dotnet/issues/1739.
                    .AddOpenTelemetry(options =>
                    {
                        options.AttachLogsToActivityEvent();
                        options.IncludeScopes = true;
                        options.IncludeFormattedMessage = true;
                        options.ParseStateValues = true;
                    })
                    .AddSerilog(new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger(), dispose: true);
            });

            return services;
        }

        /// <summary>
        /// Checks whether the given <paramref name="configuration"/> declares a Serilog file sink.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance to check.</param>
        /// <returns>True, if a Serilog file sink has been configured; false, otherwise.</returns>
        public static bool IsFileSinkConfigured(IConfiguration configuration)
        {
            // ReSharper disable once SettingNotFoundInConfiguration
            IEnumerable<KeyValuePair<string, string>> configuredSerilogSinks =
                configuration.GetSection("Serilog:Using").AsEnumerable();

            bool isSerilogFileSinkConfigured =
                configuredSerilogSinks.Any(sink => "Serilog.Sinks.File".Equals(sink.Value));

            return isSerilogFileSinkConfigured;
        }

        private static void SetFileSinkDestinationFolder(IConfiguration configuration)
        {
            if (!IsFileSinkConfigured(configuration))
            {
                return;
            }

            const string logsHomeEnvironmentVariableName = Logging.LogsHomeEnvironmentVariable;
            string logsHomeDirectoryPath = Environment.GetEnvironmentVariable(logsHomeEnvironmentVariableName);

            if (!string.IsNullOrWhiteSpace(logsHomeDirectoryPath) && Directory.Exists(logsHomeDirectoryPath))
            {
                return;
            }

            var currentWorkingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            DirectoryInfo logsHomeDirectory = currentWorkingDirectory.CreateSubdirectory("Logs");
            Environment.SetEnvironmentVariable(logsHomeEnvironmentVariableName, logsHomeDirectory.FullName);
        }
    }
}
