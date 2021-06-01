namespace Todo.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using Authorization;

    using ExceptionHandling;

    using Logging;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;

    using Models;

    using Persistence;

    using Serilog;

    using ILogger = Microsoft.Extensions.Logging.ILogger;

    /// <summary>
    /// Starts this ASP.NET Core application.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const string LogsHomeEnvironmentVariable = "LOGS_HOME";

        private IConfiguration Configuration { get; }

        private IWebHostEnvironment WebHostEnvironment { get; }

        private string ApplicationName { get; }

        private bool IsHttpLoggingEnabled { get; }

        private bool IsMiniProfilerEnabled { get; }

        private bool IsSerilogFileSinkConfigured { get; }

        private bool IsSerilogApplicationInsightsSinkConfigured { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to be used for setting up this application.</param>
        /// <param name="webHostEnvironment">The web hosting environment running this application.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            WebHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            ApplicationName = $"Todo ASP.NET Core Web API [environment={WebHostEnvironment.EnvironmentName}]";

            IsMiniProfilerEnabled = Configuration.GetValue<bool>("MiniProfiler:Enabled");
            IsHttpLoggingEnabled = Configuration.GetValue<bool>("HttpLogging:Enabled");

            IEnumerable<KeyValuePair<string, string>> configuredSerilogSinks =
                Configuration.GetSection("Serilog:Using").AsEnumerable().ToList();

            IsSerilogFileSinkConfigured = configuredSerilogSinks.Any(sink => "Serilog.Sinks.File".Equals(sink.Value));

            IsSerilogApplicationInsightsSinkConfigured =
                configuredSerilogSinks.Any(sink => "Serilog.Sinks.ApplicationInsights".Equals(sink.Value));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            ConfigureLogging(services);
            ConfigureSecurity(services);
            ConfigureMiniProfiler(services);
            ConfigureWebApi(services);
            ConfigureApplicationServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder applicationBuilder, IHostApplicationLifetime hostApplicationLifetime,
            ILogger<Startup> logger)
        {
            logger.LogInformation("{ApplicationName} is starting ...", ApplicationName);

            if (IsSerilogFileSinkConfigured)
            {
                logger.LogInformation(
                    "The {LogsHomeEnvironmentVariable} environment variable now points to directory: {LogsHomeDirectory}",
                    LogsHomeEnvironmentVariable, Environment.GetEnvironmentVariable(LogsHomeEnvironmentVariable));
            }

            applicationBuilder.UseConversationId();

            if (IsHttpLoggingEnabled)
            {
                applicationBuilder.UseHttpLogging();
            }

            // The exception handling middleware must be added inside the ASP.NET Core request pipeline
            // as soon as possible to ensure any unhandled exception is eventually handled.
            applicationBuilder.UseExceptionHandler(new ExceptionHandlerOptions
            {
                ExceptionHandler = CustomExceptionHandler.HandleException,
                AllowStatusCode404Response = true
            });

            if (IsMiniProfilerEnabled)
            {
                applicationBuilder.UseMiniProfiler();
            }

            applicationBuilder.UseHttpsRedirection();
            applicationBuilder.UseRouting();
            applicationBuilder.UseAuthentication();
            applicationBuilder.UseAuthorization();
            applicationBuilder.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            hostApplicationLifetime.ApplicationStarted.Register(() => OnApplicationStarted(applicationBuilder, logger));
            hostApplicationLifetime.ApplicationStopping.Register(() => OnApplicationStopping(logger));
            hostApplicationLifetime.ApplicationStopped.Register(() => OnApplicationStopped(logger));
        }

        private void ConfigureApplicationInsights(IServiceCollection services)
        {
            if (IsSerilogApplicationInsightsSinkConfigured)
            {
                var applicationInsightsOptions = new ApplicationInsightsOptions();
                Configuration.Bind(applicationInsightsOptions);

                services.AddApplicationInsightsTelemetry(applicationInsightsOptions.InstrumentationKey);
            }
        }

        private void ConfigureHttpLogging(IServiceCollection services)
        {
            if (IsHttpLoggingEnabled)
            {
                // Register service with 2 interfaces.
                // See more here: https://andrewlock.net/how-to-register-a-service-with-multiple-interfaces-for-in-asp-net-core-di/.
                services
                    .AddSingleton<LoggingService>()
                    .AddSingleton<IHttpObjectConverter>(serviceProvider =>
                        serviceProvider.GetRequiredService<LoggingService>())
                    .AddSingleton<IHttpContextLoggingHandler>(serviceProvider =>
                        serviceProvider.GetRequiredService<LoggingService>());
            }
        }

        private void ConfigureLogging(IServiceCollection services)
        {
            ConfigureApplicationInsights(services);
            ConfigureHttpLogging(services);

            services.AddLogging(loggingBuilder =>
            {
                if (IsSerilogFileSinkConfigured)
                {
                    string logsHomeDirectoryPath = Environment.GetEnvironmentVariable(LogsHomeEnvironmentVariable);

                    if (string.IsNullOrWhiteSpace(logsHomeDirectoryPath) || !Directory.Exists(logsHomeDirectoryPath))
                    {
                        var currentWorkingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
                        DirectoryInfo logsHomeDirectory = currentWorkingDirectory.CreateSubdirectory("Logs");
                        Environment.SetEnvironmentVariable(LogsHomeEnvironmentVariable, logsHomeDirectory.FullName);
                    }
                }

                loggingBuilder.ClearProviders();

                loggingBuilder.AddSerilog(new LoggerConfiguration()
                    .ReadFrom.Configuration(Configuration)
                    .CreateLogger(), dispose: true);
            });
        }

        private void ConfigureSecurity(IServiceCollection services)
        {
            // Display personally identifiable information only during development
            IdentityModelEventSource.ShowPII = WebHostEnvironment.IsDevelopment();

            // Configure authentication & authorization using JSON web tokens
            IConfigurationSection generateJwtOptions = Configuration.GetSection("GenerateJwt");

            // ReSharper disable once SettingNotFoundInConfiguration
            string tokenIssuer = generateJwtOptions.GetValue<string>("Issuer");

            // ReSharper disable once SettingNotFoundInConfiguration
            string tokenAudience = generateJwtOptions.GetValue<string>("Audience");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                // ReSharper disable once SettingNotFoundInConfiguration
                                Encoding.UTF8.GetBytes(generateJwtOptions.GetValue<string>("Secret"))),
                        ValidateIssuer = true,
                        ValidIssuer = tokenIssuer,
                        ValidateAudience = true,
                        ValidAudience = tokenAudience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        // Ensure the User.Identity.Name is set to the user identifier and not to the user name.
                        NameClaimType = ClaimTypes.NameIdentifier
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                // Add a custom HTTP header to the response in case the application detected that the
                                // current request is accompanied by an expired security token.
                                context.Response.Headers.Add("Token-Expired", "true");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.TodoItems.GetTodoItems,
                    policy => policy.Requirements.Add(new HasScopeRequirement("get:todo", tokenIssuer)));

                options.AddPolicy(Policies.TodoItems.CreateTodoItem,
                    policy => policy.Requirements.Add(new HasScopeRequirement("create:todo", tokenIssuer)));

                options.AddPolicy(Policies.TodoItems.UpdateTodoItem,
                    policy => policy.Requirements.Add(new HasScopeRequirement("update:todo", tokenIssuer)));

                options.AddPolicy(Policies.TodoItems.DeleteTodoItem,
                    policy => policy.Requirements.Add(new HasScopeRequirement("delete:todo", tokenIssuer)));
            });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            // Configure options used for customizing generating JWT tokens.
            // Options pattern: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0.
            services.Configure<GenerateJwtOptions>(generateJwtOptions);
        }

        private void ConfigureMiniProfiler(IServiceCollection services)
        {
            // Configure MiniProfiler for Web API and EF Core.
            // Based on: https://dotnetthoughts.net/using-miniprofiler-in-aspnetcore-webapi/.
            if (IsMiniProfilerEnabled)
            {
                services
                    .AddMemoryCache()
                    .AddMiniProfiler(options =>
                    {
                        // MiniProfiler URLs (assuming options.RouteBasePath has been set to '/miniprofiler')
                        // - show all requests:         /miniprofiler/results-index
                        // - show current request:      /miniprofiler/results
                        // - show all requests as JSON: /miniprofiler/results-list
                        options.RouteBasePath = Configuration.GetValue<string>("MiniProfiler:RouteBasePath");
                        options.EnableServerTimingHeader = true;
                    })
                    .AddEntityFramework();
            }
        }

        private static void ConfigureWebApi(IServiceCollection services)
        {
            // Configure ASP.NET Web API services.
            services
                .AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var validationProblemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Title = "One or more model validation error have occurred",
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = "See the errors property for more details",
                            Instance = context.HttpContext.Request.Path,
                            Extensions = {{"TraceId", context.HttpContext.TraceIdentifier}}
                        };

                        return new UnprocessableEntityObjectResult(validationProblemDetails)
                        {
                            ContentTypes = {"application/problem+json"}
                        };
                    };
                });
        }

        private void ConfigureApplicationServices(IServiceCollection services)
        {
            // Configure options used when handling exceptions.
            services.Configure<ExceptionHandlingOptions>(Configuration.GetSection("ExceptionHandling"));
        }

        private void OnApplicationStarted(IApplicationBuilder applicationBuilder, ILogger logger)
        {
            MigrateDatabase(applicationBuilder, logger);
            logger.LogInformation("{ApplicationName} has started", ApplicationName);
        }

        private void OnApplicationStopping(ILogger logger)
        {
            logger.LogInformation("{ApplicationName} is stopping ...", ApplicationName);
        }

        private void OnApplicationStopped(ILogger logger)
        {
            logger.LogInformation("{ApplicationName} has stopped", ApplicationName);
        }

        private void MigrateDatabase(IApplicationBuilder applicationBuilder, ILogger logger)
        {
            bool shouldMigrateDatabase = Configuration.GetValue<bool>("MigrateDatabase");

            if (!shouldMigrateDatabase)
            {
                logger.LogInformation("Migrating database has been turned off");

                return;
            }

            logger.LogInformation("Migrating database has been turned on");

            using var serviceScope =
                applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            using var todoDbContext = serviceScope.ServiceProvider.GetRequiredService<TodoDbContext>();
            string database = todoDbContext.Database.GetDbConnection().Database;

            logger.LogInformation("About to migrate database {DatabaseName} ...", database);

            try
            {
                todoDbContext.Database.Migrate();
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Failed to migrate database {DatabaseName}", database);

                throw;
            }

            logger.LogInformation("Database {DatabaseName} has been migrated", database);
        }
    }
}
