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
        /// <summary>
        /// Runs Todo ASP.NET Core Web API.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // Configure Serilog logger for this console application
            Logger logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                logger.Information("About to start Todo ASP.NET Core Web API ...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                logger.Fatal(exception, "Todo ASP.NET Core Web API failed to start");
            }
            finally
            {
                logger.Dispose();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder hostBuilder =
                Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                    {
                        configurationBuilder.Sources.Clear();
                        configurationBuilder.AddJsonFile("appsettings.json", false)
                            .AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json",
                                true)
                            .AddEnvironmentVariables()
                            .AddCommandLine(args);
                    })
                    .ConfigureWebHostDefaults(localHostBuilder => { localHostBuilder.UseStartup<Startup>(); });
            return hostBuilder;
        }
    }
}