using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TodoWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>()
                   .ConfigureLogging((hostingContext, logging) =>
                   {
                       // https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore
                       var log4NetProviderOptions = hostingContext.Configuration.GetSection("Log4NetCore").Get<Log4NetProviderOptions>();
                       logging.AddLog4Net(log4NetProviderOptions);

                       // https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore#net-core-20---logging-debug-level-messages
                       logging.SetMinimumLevel(LogLevel.Debug);
                   });
    }
}
