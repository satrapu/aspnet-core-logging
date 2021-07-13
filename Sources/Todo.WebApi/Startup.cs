namespace Todo.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using Authorization;

    using Commons;

    using ExceptionHandling;

    using Logging.ApplicationInsights;
    using Logging.Http;
    using Logging.Serilog;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;

    using Models;

    using Profiling;

    using ILogger = Microsoft.Extensions.Logging.ILogger;

    /// <summary>
    /// Starts this ASP.NET Core application.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const string ApplicationName = "Todo ASP.NET Core Web API";

        private IConfiguration Configuration { get; }

        private IWebHostEnvironment WebHostEnvironment { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to be used for setting up this application.</param>
        /// <param name="webHostEnvironment">The web host environment running this application.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            WebHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        /// <summary>
        /// This method gets called by the runtime; use it to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureExceptionHandling(services);
            ConfigureLogging(services);
            ConfigureProfiling(services);
            ConfigureSecurity(services);
            ConfigureWebApi(services);
        }

        /// <summary>
        /// This method gets called by the runtime; use it to configure the ASP.NET Core request processing pipeline.
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <param name="hostApplicationLifetime"></param>
        /// <param name="applicationStartedEventListeners"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder applicationBuilder, IHostApplicationLifetime hostApplicationLifetime,
            IEnumerable<IApplicationStartedEventListener> applicationStartedEventListeners, ILogger<Startup> logger)
        {
            logger.LogInformation("Configuring ASP.NET Core request processing pipeline ...");

            string logsHomeEnvironmentVariableName = Constants.Logging.LogsHomeEnvironmentVariable;

            logger.LogInformation(
                "The {LogsHomeEnvironmentVariable} environment variable now points to directory: [{LogsHomeDirectory}]",
                logsHomeEnvironmentVariableName, Environment.GetEnvironmentVariable(logsHomeEnvironmentVariableName));

            applicationBuilder
                .UseConversationId()
                .UseHttpLogging(Configuration)
                .UseExceptionHandler(new ExceptionHandlerOptions
                {
                    ExceptionHandler = CustomExceptionHandler.HandleException,
                    AllowStatusCode404Response = true
                })
                .UseMiniProfiler(Configuration)
                .UseHttpsRedirection()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });

            hostApplicationLifetime.ApplicationStarted.Register(() =>
                OnApplicationStarted(applicationStartedEventListeners, hostApplicationLifetime, logger));

            hostApplicationLifetime.ApplicationStopping.Register(() => OnApplicationStopping(logger));
            hostApplicationLifetime.ApplicationStopped.Register(() => OnApplicationStopped(logger));
        }

        private void ConfigureLogging(IServiceCollection services)
        {
            services
                .ActivateApplicationInsights(Configuration)
                .ActivateSerilog(Configuration);
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

        private void ConfigureProfiling(IServiceCollection services)
        {
            services.ActivateMiniProfiler(Configuration);
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
                            Title = "One or more model validation errors have occurred",
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

        private void ConfigureExceptionHandling(IServiceCollection services)
        {
            services.Configure<ExceptionHandlingOptions>(Configuration.GetSection("ExceptionHandling"));
        }

        private void OnApplicationStarted(IEnumerable<IApplicationStartedEventListener> eventListeners,
            IHostApplicationLifetime hostApplicationLifetime, ILogger logger)
        {
            try
            {
                ExecuteApplicationStartedEventListeners(eventListeners, logger);
                logger.LogInformation("{ApplicationName} application has started", ApplicationName);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception,
                    "An error has occurred while executing application started event listeners");

                hostApplicationLifetime.StopApplication();
            }
        }

        private void OnApplicationStopping(ILogger logger)
        {
            logger.LogInformation("{ApplicationName} application is stopping ...", ApplicationName);
        }

        private void OnApplicationStopped(ILogger logger)
        {
            logger.LogInformation("{ApplicationName} application has stopped", ApplicationName);
        }

        private void ExecuteApplicationStartedEventListeners(
            IEnumerable<IApplicationStartedEventListener> eventListeners,
            ILogger logger)
        {
            if (eventListeners == null)
            {
                throw new ArgumentNullException(nameof(eventListeners));
            }

            foreach (IApplicationStartedEventListener eventListener in eventListeners)
            {
                string eventListenerName = eventListener.GetType().AssemblyQualifiedName;

                logger.LogInformation(
                    "About to execute application started event listener: [{ApplicationStartedEventListener}] ...",
                    eventListenerName);

                eventListener.OnApplicationStarted();

                logger.LogInformation(
                    "Application started event listener: [{ApplicationStartedEventListener}] has been executed successfully",
                    eventListenerName);
            }
        }
    }
}
