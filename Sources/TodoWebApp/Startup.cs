using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoWebApp.Logging;
using TodoWebApp.Models;
using TodoWebApp.Services;

namespace TodoWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure logging
            services.AddLogging(loggingBuilder =>
            {
                // https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore
                var log4NetProviderOptions = Configuration.GetSection("Log4NetCore").Get<Log4NetProviderOptions>();
                loggingBuilder.AddLog4Net(log4NetProviderOptions);

                // https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore#net-core-20---logging-debug-level-messages
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            });

            // Configure EF Core
            services.AddDbContext<TodoDbContext>((serviceProvider, dbContextOptionsBuilder) =>
                     {
                         dbContextOptionsBuilder.UseInMemoryDatabase("TodoList")
                                                .EnableSensitiveDataLogging()
                                                .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
                     });

            // Configure ASP.NET Web API
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Configure application services
            services.AddScoped<ITodoService, TodoService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder applicationBuilder, IHostingEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                applicationBuilder.UseDeveloperExceptionPage();
                applicationBuilder.UseDatabaseErrorPage();
            }
            else
            {
                applicationBuilder.UseHsts();
            }

            applicationBuilder.UseMiddleware<LoggingMiddleware>()
                              .UseHttpsRedirection()
                              .UseMvc();
        }
    }
}
