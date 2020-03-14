using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;
using Todo.Persistence;

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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            IConfigurationRoot testConfiguration = configurationBuilder.AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{EnvironmentName}.json", false)
                .AddEnvironmentVariables()
                .Build();

            builder.UseConfiguration(testConfiguration);
            builder.UseEnvironment(EnvironmentName);
            builder.ConfigureTestServices(services =>
            {
                // Ensure an implementation of IHttpClientFactory interface can be injected at a later time
                services.AddHttpClient();

                SetupDbContext(services);
                SetupDatabase(services);
            });
        }

        public async Task<HttpClient> CreateClientWithJwtToken()
        {
            string accessToken = await GetAccessToken().ConfigureAwait(false);
            HttpClient httpClient = CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return httpClient;
        }

        private async Task<string> GetAccessToken()
        {
            IConfiguration configuration = base.Services.GetRequiredService<IConfiguration>();
            // ReSharper disable SettingNotFoundInConfiguration
            string requestUri = $"https://{configuration["Auth0:Domain"]}/oauth/token";
            string audience = configuration["Auth0:Audience"];
            string clientId = configuration["Auth0:ClientId"];
            string clientSecret = configuration["Auth0:ClientSecret"];
            // ReSharper restore SettingNotFoundInConfiguration

            using HttpClient httpClient = base.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("audience", audience)
                })
            };
            HttpResponseMessage httpResponseMessage =
                await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new CouldNotGetTokenException(httpResponseMessage);
            }

            JObject content = await httpResponseMessage.Content.ReadAsAsync<JObject>().ConfigureAwait(false);
            string accessToken = content.Value<string>("access_token");
            return accessToken;
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