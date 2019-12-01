using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Todo.WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            IWebHostBuilder webHostBuilder =
                WebHost.CreateDefaultBuilder(args)
                       .ConfigureAppConfiguration((webHostBuilderContext, configurationBuilder) =>
                       {
                           string environmentName = webHostBuilderContext.HostingEnvironment.EnvironmentName;
                           configurationBuilder.AddJsonFile("appsettings.json", false)
                                               .AddJsonFile($"appsettings.{environmentName}.json", true)
                                               .AddEnvironmentVariables()
                                               .AddCommandLine(args);
                       })
                       .UseStartup<Startup>();
            return webHostBuilder;
        }
    }
}