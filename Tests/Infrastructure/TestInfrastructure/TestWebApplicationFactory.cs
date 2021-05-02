namespace Todo.TestInfrastructure
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using Npgsql;

    using Persistence;

    using WebApi;
    using WebApi.Models;

    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private const string EnvironmentName = "IntegrationTests";
        private readonly string testDatabaseName;

        public TestWebApplicationFactory(string applicationName)
        {
            testDatabaseName = $"it--{applicationName}";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            IConfigurationRoot testConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{EnvironmentName}.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            builder.UseConfiguration(testConfiguration);
            builder.UseEnvironment(EnvironmentName);
            builder.ConfigureServices(services =>
            {
                // Don't run IHostedServices when running tests
                services.RemoveAll(typeof(IHostedService));

                // Ensure an implementation of IHttpClientFactory interface can be injected at a later time
                services.AddHttpClient();

                SetupDbContext(services);
                SetupDatabase(services);
            });
        }

        public async Task<HttpClient> CreateClientWithJwtAsync()
        {
            string accessToken = await GetAccessTokenAsync();
            HttpClient httpClient = CreateClientWithLoggingCapabilities();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return httpClient;
        }

        private HttpClient CreateClientWithLoggingCapabilities()
        {
            ILoggerFactory loggerFactory = Server.Services.GetService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<TestWebApplicationFactory>();
            HttpClient httpClient = CreateDefaultClient(new LoggingHandler(new HttpClientHandler(), logger));
            return httpClient;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var generateJwtModel = new GenerateJwtModel
            {
                UserName = $"user-{Guid.NewGuid():N}",
                Password = $"password-{Guid.NewGuid():N}",
            };

            using HttpClient httpClient = CreateClientWithLoggingCapabilities();
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("api/jwt",
                new StringContent(JsonConvert.SerializeObject(generateJwtModel), Encoding.UTF8, "application/json"));

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new CouldNotGetJwtException(httpResponseMessage);
            }

            JwtModel jwtModel =
                JsonConvert.DeserializeObject<JwtModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            return jwtModel.AccessToken;
        }

        private void SetupDbContext(IServiceCollection services)
        {
            ServiceDescriptor dbContextServiceDescriptor = services.SingleOrDefault(serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(DbContextOptions<TodoDbContext>));

            if (dbContextServiceDescriptor != null)
            {
                services.Remove(dbContextServiceDescriptor);
            }

            services.AddDbContext<TodoDbContext>((serviceProvider, dbContextOptionsBuilder) =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                // ReSharper disable once SettingNotFoundInConfiguration
                var testConnectionString = configuration.GetConnectionString("TodoForIntegrationTests");
                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(testConnectionString)
                {
                    Database = testDatabaseName
                };

                LogConnectionString(connectionStringBuilder.ConnectionString, serviceProvider);

                dbContextOptionsBuilder.UseNpgsql(connectionStringBuilder.ConnectionString)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
            });
        }

        private static void LogConnectionString(string originalConnectionString, IServiceProvider serviceProvider)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(originalConnectionString)
            {
                Password = new string('*', 5)
            };

            ILogger logger = serviceProvider.GetRequiredService<ILogger<TestWebApplicationFactory>>();
            logger.LogInformation("Will use connection string: {ConnectionStringWithObfuscatedPassword}",
                connectionStringBuilder.ConnectionString);
        }

        private static void SetupDatabase(IServiceCollection services)
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope serviceScope = serviceProvider.CreateScope();
            TodoDbContext todoDbContext = serviceScope.ServiceProvider.GetRequiredService<TodoDbContext>();
            ILogger logger = serviceProvider.GetRequiredService<ILogger<TestWebApplicationFactory>>();
            string databaseName = todoDbContext.Database.GetDbConnection().Database;
            logger.LogInformation("About to setup test database {TestDatabaseName} ...", databaseName);

            try
            {
                RunMigrations(databaseName, todoDbContext, logger);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to run migrations against test database {TestDatabaseName}",
                    databaseName);
                throw;
            }

            logger.LogInformation("Test database {TestDatabaseName} has been successfully setup", databaseName);
        }

        private static void RunMigrations(string databaseName, TodoDbContext todoDbContext, ILogger logger)
        {
            logger.LogInformation("About to delete test database {TestDatabaseName} ...", databaseName);
            bool hasDeleteDatabase = todoDbContext.Database.EnsureDeleted();

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (hasDeleteDatabase)
            {
                logger.LogInformation("Test database {TestDatabaseName} has been successfully deleted", databaseName);
            }
            else
            {
                logger.LogInformation("Could not find any test database {TestDatabaseName} to delete", databaseName);
            }

            logger.LogInformation("About to run migrations against test database {TestDatabaseName} ...", databaseName);
            IMigrator databaseMigrator = todoDbContext.GetInfrastructure().GetRequiredService<IMigrator>();
            databaseMigrator.Migrate();
            logger.LogInformation("Migrations have been successfully run against test database {TestDatabaseName}",
                databaseName);
        }
    }

    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger logger;

        public LoggingHandler(HttpMessageHandler innerHandler, ILogger logger) : base(innerHandler)
        {
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("-- REQUEST: BEGIN --");
            stringBuilder.AppendLine($"{request}\nContent:\n\t");

            if (request.Content != null)
            {
                stringBuilder.AppendLine(await request.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                stringBuilder.AppendLine("N/A");
            }

            stringBuilder.AppendLine("-- REQUEST: END --");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            stringBuilder.AppendLine("-- RESPONSE: BEGIN --");
            stringBuilder.AppendLine($"{response}\nContent:\n\t");

            if (request.Content != null)
            {
                stringBuilder.AppendLine(await response.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                stringBuilder.AppendLine("N/A");
            }

            stringBuilder.AppendLine("-- RESPONSE: END --");

            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            logger.LogInformation(stringBuilder.ToString());

            return response;
        }
    }
}
