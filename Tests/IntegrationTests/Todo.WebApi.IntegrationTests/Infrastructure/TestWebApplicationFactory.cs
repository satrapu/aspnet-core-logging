using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Todo.Persistence;
using Todo.WebApi.Models;

namespace Todo.WebApi.Infrastructure
{
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private const string EnvironmentName = "IntegrationTests";
        private readonly string testDatabaseName;

        public TestWebApplicationFactory(string applicationName)
        {
            testDatabaseName = $"it--{applicationName}";
        }

        protected override void ConfigureWebHost(IWebHostBuilder webHostBuilder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            IConfigurationRoot testConfiguration = configurationBuilder.AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{EnvironmentName}.json", false)
                .AddEnvironmentVariables()
                .Build();

            webHostBuilder.UseConfiguration(testConfiguration);
            webHostBuilder.UseEnvironment(EnvironmentName);
            webHostBuilder.ConfigureTestServices(services =>
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
            string accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            HttpClient httpClient = CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return httpClient;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var generateJwtModel = new GenerateJwtModel
            {
                UserName = $"user-{Guid.NewGuid():N}",
                Password = $"password-{Guid.NewGuid():N}",
            };

            using HttpClient httpClient = CreateClient();
            HttpResponseMessage httpResponseMessage =
                await httpClient.PostAsJsonAsync("api/jwt", generateJwtModel).ConfigureAwait(false);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new CouldNotGetJwtException(httpResponseMessage);
            }

            JwtModel jwtModel =
                await httpResponseMessage.Content.ReadAsAsync<JwtModel>().ConfigureAwait(false);
            return jwtModel.AccessToken;
        }

        private void SetupDbContext(IServiceCollection services)
        {
            ILogger logger = services.BuildServiceProvider().GetRequiredService<ILogger<TestWebApplicationFactory>>();
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

                logger.LogInformation("Integration tests will use the following connection string: {ConnectionString}",
                    connectionStringBuilder.ConnectionString);

                dbContextOptionsBuilder.UseNpgsql(connectionStringBuilder.ConnectionString)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
            });
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
                logger.LogError("Failed to run migrations against test database {TestDatabaseName}", exception,
                    databaseName);
                throw;
            }

            logger.LogInformation("Test database {TestDatabaseName} has been successfully setup", databaseName);
        }

        private static void RunMigrations(string databaseName, TodoDbContext todoDbContext, ILogger logger)
        {
            logger.LogInformation("About to delete test database {TestDatabaseName} ...", databaseName);
            bool hasDeleteDatabase = todoDbContext.Database.EnsureDeleted();

            logger.LogInformation(
                hasDeleteDatabase
                    ? "Test database {TestDatabaseName} has been successfully deleted"
                    : "Could not find any test database {TestDatabaseName} to delete", databaseName);

            logger.LogInformation("About to run migrations against test database {TestDatabaseName} ...", databaseName);
            IMigrator databaseMigrator = todoDbContext.GetInfrastructure().GetRequiredService<IMigrator>();
            databaseMigrator.Migrate();
            logger.LogInformation("Migrations have been successfully run against test database {TestDatabaseName}",
                databaseName);
        }
    }
}