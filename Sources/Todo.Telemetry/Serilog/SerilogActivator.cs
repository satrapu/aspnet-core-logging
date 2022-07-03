namespace Todo.Telemetry.Serilog
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
    using Todo.Telemetry.OpenTelemetry;

    /// <summary>
    /// Contains extension methods used for integrating Serilog with this application.
    /// </summary>
    public static class SerilogActivator
    {
        /// <summary>
        /// Adds Serilog logging provider to the given <paramref name="services"/> instance.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The given <paramref name="services"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="services"/>
        /// or <paramref name="configuration"/> is null.</exception>
        public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddLogging(loggingBuilder =>
            {
                if (IsFileSinkConfigured(configuration))
                {
                    SetFileSinkDestinationFolder();
                }

                loggingBuilder
                     .ClearProviders()
                     .AddOpenTelemetry(configuration)
                     .AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration)
                         .CreateLogger(), dispose: true);
            });

            return services;
        }

        /// <summary>
        /// Checks whether the given <paramref name="configuration"/> declares a Serilog file sink.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance to check.</param>
        /// <returns>True, if a Serilog file sink has been configured; false, otherwise.</returns>
        internal static bool IsFileSinkConfigured(IConfiguration configuration)
        {
            // ReSharper disable once SettingNotFoundInConfiguration
            IEnumerable<KeyValuePair<string, string>> configuredSerilogSinks =
                configuration.GetSection(SerilogConstants.SectionNames.Using).AsEnumerable();

            bool isSerilogFileSinkConfigured =
                configuredSerilogSinks.Any(sink => SerilogConstants.SinkShortNames.File.Equals(sink.Value));

            return isSerilogFileSinkConfigured;
        }

        private static void SetFileSinkDestinationFolder()
        {
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
