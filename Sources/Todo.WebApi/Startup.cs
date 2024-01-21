namespace Todo.WebApi
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using Authorization;

    using Autofac;

    using ApplicationFlows.DependencyInjection;

    using Commons.Constants;
    using Commons.Diagnostics;

    using ExceptionHandling;
    using ExceptionHandling.Configuration;

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

    using Persistence;

    using Telemetry.DependencyInjection;
    using Telemetry.Http;
    using Telemetry.OpenTelemetry;
    using Telemetry.Serilog;

    /// <summary>
    /// Starts this ASP.NET Core application.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const string HttpLoggingEnabledConfigurationLookupKey = "HttpLogging:Enabled";

        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// Creates a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to be used for setting up this application.</param>
        /// <param name="webHostEnvironment">The web host environment running this application.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        public void ConfigureContainer(HostBuilderContext hostBuilderContext, ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterModule(new TelemetryModule
                {
                    EnableHttpLogging = hostBuilderContext.Configuration.GetValue<bool>(HttpLoggingEnabledConfigurationLookupKey)
                })
                .RegisterModule(new ApplicationFlowsModule
                {
                    EnvironmentName = hostBuilderContext.HostingEnvironment.EnvironmentName,
                    ApplicationConfiguration = hostBuilderContext.Configuration
                });
        }

        /// <summary>
        /// This method gets called by the runtime; use it to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureExceptionHandling(services);
            ConfigureTelemetry(services);
            ConfigureSecurity(services);
            ConfigureWebApi(services);
        }

        /// <summary>
        /// This method gets called by the runtime; use it to configure the ASP.NET Core request processing pipeline.
        /// </summary>
        /// <param name="applicationBuilder">Configures ASP.NET Core request processing pipeline.</param>
        /// <param name="hostApplicationLifetime">Notifies about application events.</param>
        /// <param name="serviceProvider">Fetches services from the DI container.</param>
        /// <param name="logger">Logs messages generated by this method.</param>
        // ReSharper disable once UnusedMember.Global
        public void Configure
        (
            IApplicationBuilder applicationBuilder,
            IHostApplicationLifetime hostApplicationLifetime,
            IServiceProvider serviceProvider,
            ILogger<Startup> logger
        )
        {
            logger.LogInformation("Configuring ASP.NET Core request processing pipeline ...");

            bool isDevelopmentEnvironment = EnvironmentNames.Development.Equals(webHostEnvironment.EnvironmentName);
            bool isIntegrationTestsEnvironment = EnvironmentNames.IntegrationTests.Equals(webHostEnvironment.EnvironmentName);
            bool isAcceptanceTestsEnvironment = EnvironmentNames.AcceptanceTests.Equals(webHostEnvironment.EnvironmentName);

            applicationBuilder
                .UseConversationId()
                .UseHttpLogging(configuration)
                .UseExceptionHandler(new ExceptionHandlerOptions
                {
                    ExceptionHandler = CustomExceptionHandler.HandleException,
                    AllowStatusCode404Response = true
                });

            if (!isDevelopmentEnvironment && !isIntegrationTestsEnvironment && !isAcceptanceTestsEnvironment)
            {
                applicationBuilder.UseHttpsRedirection();
            }

            applicationBuilder
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers().RequireAuthorization();
                    endpoints.MapHealthChecks("health").AllowAnonymous();
                });

            hostApplicationLifetime.ApplicationStarted.Register(() => OnApplicationStarted());
            hostApplicationLifetime.ApplicationStopping.Register(() => OnApplicationStopping(logger));
            hostApplicationLifetime.ApplicationStopped.Register(() => OnApplicationStopped(logger));

            logger.LogInformation("ASP.NET Core request processing pipeline has been configured");
        }

        private void ConfigureTelemetry(IServiceCollection services)
        {
            services
                .AddLogging(loggingBuilder => loggingBuilder.ClearProviders())
                .AddSerilog(configuration)
                .AddOpenTelemetry(configuration, webHostEnvironment)
                .AddHealthChecks().AddDbContextCheck<TodoDbContext>();
        }

        private void ConfigureSecurity(IServiceCollection services)
        {
            // Display personally identifiable information only during development
            IdentityModelEventSource.ShowPII = webHostEnvironment.IsDevelopment();

            // Configure authentication & authorization using JSON web tokens
            IConfigurationSection generateJwtOptions = configuration.GetSection("GenerateJwt");

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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(generateJwtOptions.GetValue<string>("Secret"))),
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
                                context.Response.Headers.Append("Token-Expired", "true");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.TodoItems.GetTodoItems, policy => policy.Requirements.Add(new HasScopeRequirement("get:todo", tokenIssuer)));
                options.AddPolicy(Policies.TodoItems.CreateTodoItem, policy => policy.Requirements.Add(new HasScopeRequirement("create:todo", tokenIssuer)));
                options.AddPolicy(Policies.TodoItems.UpdateTodoItem, policy => policy.Requirements.Add(new HasScopeRequirement("update:todo", tokenIssuer)));
                options.AddPolicy(Policies.TodoItems.DeleteTodoItem, policy => policy.Requirements.Add(new HasScopeRequirement("delete:todo", tokenIssuer)));
            });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            // Configure options used for customizing generating JWT tokens.
            // Options pattern: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-7.0.
            services.Configure<GenerateJwtOptions>(generateJwtOptions);
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
                            Extensions =
                            {
                                {
                                    "TraceId", context.HttpContext.TraceIdentifier
                                }
                            }
                        };

                        return new UnprocessableEntityObjectResult(validationProblemDetails)
                        {
                            ContentTypes =
                            {
                                "application/problem+json"
                            }
                        };
                    };
                });
        }

        private void ConfigureExceptionHandling(IServiceCollection services)
        {
            services.Configure<ExceptionHandlingOptions>(configuration.GetSection("ExceptionHandling"));
        }

        private void OnApplicationStarted()
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            using Activity _ = ActivitySources.TodoWebApi.StartActivity("Application has started");
        }

        private void OnApplicationStopping(ILogger logger)
        {
            logger.LogInformation("Application [{ApplicationName}] is about to stop on environment [{EnvironmentName}] ...",
                webHostEnvironment.ApplicationName, webHostEnvironment.EnvironmentName);

            // ReSharper disable once ExplicitCallerInfoArgument
            using Activity _ = ActivitySources.TodoWebApi.StartActivity("Application is about to stop");
        }

        private void OnApplicationStopped(ILogger logger)
        {
            logger.LogInformation("Application [{ApplicationName}] has stopped on environment [{EnvironmentName}]",
                webHostEnvironment.ApplicationName, webHostEnvironment.EnvironmentName);

            // ReSharper disable once ExplicitCallerInfoArgument
            using Activity _ = ActivitySources.TodoWebApi.StartActivity("Application has stopped");
        }
    }
}
