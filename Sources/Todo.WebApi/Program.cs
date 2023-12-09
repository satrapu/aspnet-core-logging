namespace Todo.WebApi
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using ApplicationFlows.DependencyInjection;

    using Autofac;
    using Autofac.Extensions.DependencyInjection;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    using Serilog;
    using Serilog.Core;

    using Telemetry.DependencyInjection;

    /// <summary>
    /// Runs an application used for managing user todo items (aka user tasks).
    /// </summary>
    [SuppressMessage("ReSharper", "S1135", Justification = "The todo word represents an entity")]
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private const string HttpLoggingEnabledConfigurationLookupKey = "HttpLogging:Enabled";
        private static readonly Logger Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        /// <summary>
        /// The entry point for running the application.
        /// </summary>
        /// <param name="args">The command line arguments used when invoking the application executable.</param>
        public static async Task Main(string[] args)
        {
            try
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception, "Application failed to start");
                throw;
            }
            finally
            {
                await Logger.DisposeAsync();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            Logger.Information("Configuring the host builder needed to run the application ...");

            IHostBuilder hostBuilder =
                Host.CreateDefaultBuilder(args)
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                    {
                        configurationBuilder.Sources.Clear();

                        configurationBuilder
                            .SetBasePath(hostBuilderContext.HostingEnvironment.ContentRootPath)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json",
                                optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .AddCommandLine(args);
                    })
                    .ConfigureContainer<ContainerBuilder>((hostBuilderContext, containerBuilder) =>
                    {
                        // The purpose of this method is to configure Autofac as replacement for the default
                        // ASP.NET Core service provider.
                        // This method is not present inside Startup class (where it should), due to a known issue which
                        // prohibits injecting mock services, as seen here:
                        // https://github.com/dotnet/aspnetcore/issues/14907#issuecomment-850407104.

                        containerBuilder
                            .RegisterModule(new TelemetryModule
                            {
                                EnableHttpLogging = hostBuilderContext.Configuration.GetValue<bool>(HttpLoggingEnabledConfigurationLookupKey)
                            })
                            .RegisterModule(new ApplicationFlowsModule
                            {
                                EnvironmentName = hostBuilderContext.HostingEnvironment.EnvironmentName,
                                ApplicationConfiguration = hostBuilderContext.Configuration
                            });
                    })
                    .ConfigureWebHostDefaults(localHostBuilder =>
                    {
                        localHostBuilder.SuppressStatusMessages(true);

                        // Ensure that when an error occurs during startup, host will exit.
                        // See more about capturing startup errors here:
                        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-7.0#capture-startup-errors.
                        localHostBuilder.CaptureStartupErrors(false);

                        // Ensure the application captures detailed errors.
                        // See more about detailed errors here:
                        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-7.0#detailed-errors.
                        localHostBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, bool.TrueString);

                        // See more about the Startup class here:
                        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-7.0.
                        localHostBuilder.UseStartup<Startup>();
                    });

            Logger.Information("The host builder needed to run the application has been configured");

            return hostBuilder;
        }
    }
}
