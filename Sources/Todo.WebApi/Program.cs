using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Core;

namespace Todo.WebApi
{
    /// <summary>
    /// Console application used for running Todo ASP.NET Core Web API.
    /// </summary>
    public static class Program
    {
        private static readonly Logger Logger;

        static Program()
        {
            // Configure Serilog logger for this class only.
            // This logger is different than the one defined inside Startup class, which will be used by the rest of
            // the classes found inside Todo ASP.NET Core Web API.
            Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }

        /// <summary>
        /// Runs Todo ASP.NET Core Web API.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception, "Todo ASP.NET Core Web API failed to start");
                throw;
            }
            finally
            {
                Logger.Dispose();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            Logger.Information("Configuring host builder needed to run Todo ASP.NET Core Web API ...");
            IHostBuilder hostBuilder =
                Host.CreateDefaultBuilder(args)
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
                    .ConfigureWebHostDefaults(localHostBuilder =>
                    {
                        // Ensure that when an error occurs during startup, host will exit.
                        // See more about capturing startup errors here:
                        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-3.1#capture-startup-errors.
                        localHostBuilder.CaptureStartupErrors(false);

                        // Ensure the application captures detailed errors.
                        // See more about detailed errors here:
                        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-3.1#detailed-errors.
                        localHostBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, bool.TrueString);

                        // See more about the Startup class here:
                        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-3.1.
                        localHostBuilder.UseStartup<Startup>();
                    });
            Logger.Information("Host builder needed to run Todo ASP.NET Core Web API has been configured");
            return hostBuilder;
        }
    }
}
