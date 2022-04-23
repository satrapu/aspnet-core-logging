namespace Todo.Logging.OpenTelemetry
{
    using System;
    using System.Collections.Generic;

    using global::OpenTelemetry.Resources;
    using global::OpenTelemetry.Trace;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Todo.Logging.OpenTelemetry.Configuration;

    /// <summary>
    /// Contains extension methods used for integration OpenTelemetry with this application.
    /// </summary>
    public static class OpenTelemetryActivator
    {
        private const string OpenTelemetryConfigurationSectionName = "OpenTelemetry";

        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services,
            IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(webHostEnvironment);

            OpenTelemetryOptions openTelemetryOptions = configuration.GetOpenTelemetryOptions();

            if (openTelemetryOptions.Enabled is false)
            {
                return services;
            }

            return services.AddOpenTelemetryTracing(traceProviderBuilder =>
            {
                traceProviderBuilder
                    .SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(webHostEnvironment.ApplicationName)
                            .AddAttributes(new Dictionary<string, object>
                            {
                                    { "service.instance.attributes.custom.EnvironmentName", webHostEnvironment.EnvironmentName },
                                    { "service.instance.attributes.custom.ContentRootPath", webHostEnvironment.ContentRootPath },
                                    { "service.instance.attributes.custom.WebRootPath", webHostEnvironment.WebRootPath ?? "<null>" },
                                    { "service.instance.attributes.custom.OperationSystem", Environment.OSVersion.ToString() },
                                    { "service.instance.attributes.custom.MachineName", Environment.MachineName },
                                    { "service.instance.attributes.custom.ProcessorCount", Environment.ProcessorCount },
                                    { "service.instance.attributes.custom.DotNetVersion", Environment.Version.ToString() }

                            }))
                    .AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText =
                        openTelemetryOptions.Instrumentation.EntityFrameworkCore.SetDbStatementForText;
                    })
                    .AddJaegerExporter(options =>
                    {
                        options.AgentHost = openTelemetryOptions.Exporters.Jaeger.AgentHost;
                        options.AgentPort = openTelemetryOptions.Exporters.Jaeger.AgentPort;
                    });
            });
        }

        public static ILoggingBuilder AddOpenTelemetry(this ILoggingBuilder loggingBuilder, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(loggingBuilder);
            ArgumentNullException.ThrowIfNull(configuration);

            OpenTelemetryOptions openTelemetryOptions = configuration.GetOpenTelemetryOptions();

            if (openTelemetryOptions.Enabled is false)
            {
                return loggingBuilder;
            }

            return loggingBuilder.AddOpenTelemetry(options =>
            {
                if (openTelemetryOptions.AttachLogsToActivity)
                {
                    // Ensure events produces by ILogger will be exported by Open Telemetry to Jaeger.
                    // See more here: https://github.com/open-telemetry/opentelemetry-dotnet/issues/1739.
                    options.AttachLogsToActivityEvent();
                }

                options.IncludeScopes = openTelemetryOptions.IncludeScopes;
                options.IncludeFormattedMessage = openTelemetryOptions.IncludeFormattedMessage;
                options.ParseStateValues = openTelemetryOptions.ParseStateValues;
            });
        }

        private static OpenTelemetryOptions GetOpenTelemetryOptions(this IConfiguration configuration)
        {
            var openTelemetryOptions = new OpenTelemetryOptions();
            configuration.Bind(OpenTelemetryConfigurationSectionName, openTelemetryOptions);
            return openTelemetryOptions;
        }
    }
}
