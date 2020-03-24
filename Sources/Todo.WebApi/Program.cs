using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Todo.WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder hostBuilder =
                Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                    {
                        string environmentName = hostBuilderContext.HostingEnvironment.EnvironmentName;
                        configurationBuilder.AddJsonFile("appsettings.json", false)
                            .AddJsonFile($"appsettings.{environmentName}.json", true)
                            .AddEnvironmentVariables()
                            .AddCommandLine(args);
                    })
                    .ConfigureWebHostDefaults(localHostBuilder => { localHostBuilder.UseStartup<Startup>(); });
            return hostBuilder;
        }
    }
}