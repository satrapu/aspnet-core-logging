using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Todo.Persistence;
using Todo.Services;
using Todo.WebApi.Authentication;
using Todo.WebApi.Logging;

namespace Todo.WebApi
{
    /// <summary>
    /// Starts this ASP.NET Core application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to be used for setting up this application.</param>
        /// /// <param name="webHostEnvironment">The environment where this application is hosted.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            WebHostingEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        private IConfiguration Configuration { get; }

        private IWebHostEnvironment WebHostingEnvironment { get; }

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
                var connectionString = Configuration.GetConnectionString("Todo");
                dbContextOptionsBuilder.UseNpgsql(connectionString)
                    .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());

                if (WebHostingEnvironment.IsDevelopment())
                {
                    dbContextOptionsBuilder.EnableSensitiveDataLogging();
                    dbContextOptionsBuilder.EnableDetailedErrors();
                }
            });

            // Configure ASP.NET Web API
            services.AddMvc(options =>
            {
                // In case the client requests data in an unsupported format, respond with 406 status code
                options.ReturnHttpNotAcceptable = true;

                // Needed after migrating ASP.NET Core 2.2 to 3.1
                options.EnableEndpointRouting = false;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Configure application services
            services.AddScoped<ITodoService, TodoService>();

            // Register service with 2 interfaces.
            // See more here: https://andrewlock.net/how-to-register-a-service-with-multiple-interfaces-for-in-asp-net-core-di/.
            services.AddSingleton<LoggingService>();
            services.AddSingleton<IHttpObjectConverter>(serviceProvider =>
                serviceProvider.GetRequiredService<LoggingService>());
            services.AddSingleton<IHttpContextLoggingHandler>(serviceProvider =>
                serviceProvider.GetRequiredService<LoggingService>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            bool isDevelopmentEnvironment = WebHostingEnvironment.IsDevelopment();

            if (isDevelopmentEnvironment)
            {
                applicationBuilder.UseHardCodedUser();
            }

            applicationBuilder.UseHttpLogging();

            if (isDevelopmentEnvironment)
            {
                applicationBuilder.UseDeveloperExceptionPage();
                applicationBuilder.UseDatabaseErrorPage();
            }
            else
            {
                applicationBuilder.UseHsts();
            }

            applicationBuilder.UseHttpsRedirection()
                .UseMvc();
        }
    }
}