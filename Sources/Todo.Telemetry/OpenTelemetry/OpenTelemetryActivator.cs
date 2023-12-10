namespace Todo.Telemetry.OpenTelemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Azure.Monitor.OpenTelemetry.Exporter;

    using Commons.Diagnostics;

    using Configuration;

    using global::OpenTelemetry.Resources;
    using global::OpenTelemetry.Trace;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Contains extension methods used for integrating OpenTelemetry with this application.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class OpenTelemetryActivator
    {
        private const string OpenTelemetryConfigurationSectionName = "OpenTelemetry";

        /// <summary>
        /// Adds OpenTelemetry to the given <paramref name="services"/> instance.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="webHostEnvironment">The application hosting environment.</param>
        /// <returns>The given <paramref name="services"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="services"/>,
        /// <paramref name="configuration"/> or <paramref name="webHostEnvironment"/> is null.</exception>
        public static IServiceCollection AddOpenTelemetry
        (
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment
        )
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(webHostEnvironment);

            OpenTelemetryOptions openTelemetryOptions = configuration.GetOpenTelemetryOptions();

            if (openTelemetryOptions.Enabled is false)
            {
                return services;
            }

            services
                .AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .WithCustomOptions(webHostEnvironment)
                        // @satrapu 2022-04-23: Ensure activities generated by the application will be picked up
                        // by OpenTelemetry.
                        .AddSource(ActivitySources.TodoWebApi.Name)
                        .AddAspNetCoreInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation(options =>
                        {
                            options.SetDbStatementForText = openTelemetryOptions.Instrumentation.EntityFrameworkCore.SetDbStatementForText;
                        });

                    if (openTelemetryOptions.Exporters.AzureMonitor.Enabled)
                    {
                        tracerProviderBuilder.AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                        });
                    }

                    if (openTelemetryOptions.Exporters.Jaeger.Enabled)
                    {
                        tracerProviderBuilder.AddJaegerExporter(options =>
                        {
                            options.AgentHost = openTelemetryOptions.Exporters.Jaeger.AgentHost;
                            options.AgentPort = openTelemetryOptions.Exporters.Jaeger.AgentPort;
                        });
                    }
                });

            if (openTelemetryOptions.Logging.Enabled)
            {
                services.AddLogging(loggingBuilder => loggingBuilder.AddOpenTelemetry(openTelemetryOptions));
            }

            return services;
        }

        private static TracerProviderBuilder WithCustomOptions(this TracerProviderBuilder tracerProviderBuilder, IWebHostEnvironment webHostEnvironment)
        {
            return
                tracerProviderBuilder
                    .SetResourceBuilder
                    (
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(ActivitySources.TodoWebApi.Name)
                            .AddAttributes(new Dictionary<string, object>
                            {
                                { "service.instance.attributes.custom.EnvironmentName", webHostEnvironment.EnvironmentName },
                                { "service.instance.attributes.custom.ContentRootPath", webHostEnvironment.ContentRootPath },
                                // ReSharper disable once ConstantNullCoalescingCondition
                                { "service.instance.attributes.custom.WebRootPath", webHostEnvironment.WebRootPath ?? "<null>" },
                                { "service.instance.attributes.custom.OperationSystem", Environment.OSVersion.ToString() },
                                { "service.instance.attributes.custom.MachineName", Environment.MachineName },
                                { "service.instance.attributes.custom.ProcessorCount", Environment.ProcessorCount.ToString() },
                                { "service.instance.attributes.custom.DotNetVersion", Environment.Version.ToString() }
                            })
                    );
        }

        /// <summary>
        /// Adds log events to OpenTelemetry.
        /// </summary>
        /// <param name="loggingBuilder">The application logging builder.</param>
        /// <param name="openTelemetryOptions">The options needed to configure OpenTelemetry integration.</param>
        /// <returns>The given <paramref name="loggingBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="loggingBuilder"/>
        /// or <paramref name="openTelemetryOptions"/> is null.</exception>
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static ILoggingBuilder AddOpenTelemetry(this ILoggingBuilder loggingBuilder, OpenTelemetryOptions openTelemetryOptions)
        {
            return loggingBuilder.AddOpenTelemetry(options =>
            {
                if (openTelemetryOptions.Logging.AttachLogsToActivity)
                {
                    // Ensure events produces by ILogger will be exported by Open Telemetry to the configured exporter back-end (i.e., Azure Monitor or Jaeger).
                    options.AttachLogsToActivityEvent();
                }

                options.IncludeScopes = openTelemetryOptions.Logging.IncludeScopes;
                options.IncludeFormattedMessage = openTelemetryOptions.Logging.IncludeFormattedMessage;
                options.ParseStateValues = openTelemetryOptions.Logging.ParseStateValues;
            });
        }

        private static OpenTelemetryOptions GetOpenTelemetryOptions(this IConfiguration configuration)
        {
            OpenTelemetryOptions openTelemetryOptions = new();
            configuration.Bind(OpenTelemetryConfigurationSectionName, openTelemetryOptions);
            return openTelemetryOptions;
        }
    }
}
