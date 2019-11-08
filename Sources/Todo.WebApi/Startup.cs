using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Todo.Persistence;
using Todo.Services;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

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
                dbContextOptionsBuilder.UseNpgsql(connectionString
                                                , options => 
                                                {
                                                    // See more here: https://www.npgsql.org/efcore/miscellaneous.html#execution-strategy
                                                    options.EnableRetryOnFailure(20, TimeSpan.FromSeconds(30), errorCodesToAdd: new List<string>());
                                                    // See more here: https://www.npgsql.org/efcore/miscellaneous.html#database-creation
                                                    options.UseAdminDatabase("admin_db");
                                                })
                                       .EnableSensitiveDataLogging()
                                       .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
            });

            // Configure ASP.NET Web API
            services.AddMvc(options =>
                    {
                        // In case the client requests data in an unsupported format, respond with 406 status code
                        options.ReturnHttpNotAcceptable = true;
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Configure application services
            services.AddScoped<ITodoService, TodoService>();
            services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

            // Register service with 2 interfaces.
            // See more here: https://andrewlock.net/how-to-register-a-service-with-multiple-interfaces-for-in-asp-net-core-di/.
            services.AddSingleton<LoggingService>();
            services.AddSingleton<IHttpObjectConverter>(x => x.GetRequiredService<LoggingService>());
            services.AddSingleton<IHttpContextLoggingHandler>(x => x.GetRequiredService<LoggingService>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder applicationBuilder, IHostingEnvironment environment)
        {
            // Ensure logging middleware is invoked as early as possible
            applicationBuilder.UseHttpLogging();

            if (environment.IsDevelopment())
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
