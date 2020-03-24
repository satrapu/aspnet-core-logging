using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Todo.Persistence;
using Todo.Services;
using Todo.WebApi.Authorization;
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
                if (WebHostingEnvironment.IsProduction())
                {
                    loggingBuilder.ClearProviders();
                }

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

            // Display personally identifiable information only during development
            if (WebHostingEnvironment.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                IdentityModelEventSource.ShowPII = false;
            }

            // Configure authentication & authorization using JWT tokens
            string authZeroDomain = $"https://{Configuration["Auth0:Domain"]}/";

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = authZeroDomain;
                options.Audience = Configuration["Auth0:Audience"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //  Ensures the User.Identity.Name will be set to something in cases the access token does not
                    // have a "sub" claim - see more here: https://auth0.com/docs/quickstart/backend/aspnet-core-webapi#configure-the-middleware.
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.TodoItems.GetTodoItems,
                    policy => policy.Requirements.Add(new HasScopeRequirement("get:todo", authZeroDomain)));
                options.AddPolicy(Policies.TodoItems.CreateTodoItem,
                    policy => policy.Requirements.Add(new HasScopeRequirement("create:todo", authZeroDomain)));
                options.AddPolicy(Policies.TodoItems.UpdateTodoItem,
                    policy => policy.Requirements.Add(new HasScopeRequirement("update:todo", authZeroDomain)));
                options.AddPolicy(Policies.TodoItems.DeleteTodoItem,
                    policy => policy.Requirements.Add(new HasScopeRequirement("delete:todo", authZeroDomain)));
            });
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            // Configure ASP.NET Web API services
            services.AddControllers();

            // Configure Todo Web API services
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
            applicationBuilder.UseHttpLogging();

            if (WebHostingEnvironment.IsDevelopment())
            {
                applicationBuilder.UseDeveloperExceptionPage();
                applicationBuilder.UseDatabaseErrorPage();
            }

            applicationBuilder.UseHttpsRedirection();
            applicationBuilder.UseRouting();
            applicationBuilder.UseAuthentication();
            applicationBuilder.UseAuthorization();
            applicationBuilder.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}