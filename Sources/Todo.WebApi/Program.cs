namespace Todo.WebApi
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using ApplicationFlows;

    using Autofac;
    using Autofac.Configuration;
    using Autofac.Extensions.DependencyInjection;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    using Serilog;
    using Serilog.Core;

    /// <summary>
    /// Runs an application used for managing user todo items (aka user tasks).
    /// </summary>
    [SuppressMessage("ReSharper", "S1135", Justification = "The todo word represents an entity")]
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private static readonly Logger logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        /// <summary>
        /// The entry point for running the application.
        /// </summary>
        /// <param name="args">The command line arguments used when invoking the application executable.</param>
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                logger.Fatal(exception, "Application failed to start");

                throw;
            }
            finally
            {
                logger.Dispose();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            logger.Information("Configuring host builder needed to run the application ...");

            IHostBuilder hostBuilder =
                Host.CreateDefaultBuilder(args)
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                    {
                        configurationBuilder.Sources.Clear();

                        configurationBuilder.SetBasePath(hostBuilderContext.HostingEnvironment.ContentRootPath)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json",
                                optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .AddCommandLine(args);
                    })
                    .ConfigureContainer<ContainerBuilder>((hostBuilderContext, containerBuilder) =>
                    {
                        // The purpose of this method is to configure Autofac DI container.
                        // This method is not present inside Startup class (where it should), due to a known issue which
                        // prohibits injecting mock services, as seen here:
                        // https://github.com/dotnet/aspnetcore/issues/14907#issuecomment-850407104.

                        // Load Autofac configuration from JSON files.
                        IConfigurationRoot autofacConfiguration = new ConfigurationBuilder()
                            .AddJsonFile("autofac.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"autofac.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json",
                                optional: true, reloadOnChange: true)
                            .Build();

                        // Register Autofac.Configuration.ConfigurationModule in order to read module configuration
                        // from JSON files. This is needed in order to configure PersistenceModule which depends upon
                        // the name of the connection string pointing to the underlying RDBMS and upon the name of the
                        // current ASP.NET Core web host environment.
                        // Read more about this Autofac module here:
                        // https://autofac.readthedocs.io/en/latest/configuration/xml.html#quick-start.
                        containerBuilder.RegisterModule<ApplicationFlowsModule>();
                        containerBuilder.RegisterModule(new ConfigurationModule(autofacConfiguration));
                    })
                    .ConfigureWebHostDefaults(localHostBuilder =>
                    {
                        localHostBuilder.SuppressStatusMessages(true);

                        // Ensure that when an error occurs during startup, host will exit.
                        // See more about capturing startup errors here:
                        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-5.0#capture-startup-errors.
                        localHostBuilder.CaptureStartupErrors(false);

                        // Ensure the application captures detailed errors.
                        // See more about detailed errors here:
                        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-5.0#detailed-errors.
                        localHostBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, bool.TrueString);

                        // See more about the Startup class here:
                        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-5.0.
                        localHostBuilder.UseStartup<Startup>();
                    });

            logger.Information("Host builder needed to run the application has been configured");

            return hostBuilder;
        }
    }
}
