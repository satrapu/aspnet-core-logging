namespace Todo.WebApi.TestInfrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Autofac;

    using Commons.Constants;
    using Commons.StartupLogic;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Models;

    using Npgsql;

    /// <summary>
    /// A <see cref="WebApplicationFactory{TEntryPoint}"/> implementation to be used for running integration tests.
    /// <br/>
    /// Each instance of this class will use its own database to ensure isolation at test class level.
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private static readonly TimeSpan RunStartupLogicTimeout = TimeSpan.FromSeconds(30);
        private Action<ContainerBuilder> setupMockServicesAction;
        private readonly string applicationName;
        private readonly string environmentName;

        private TestWebApplicationFactory(string applicationName, string environmentName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException(message: "Application name cannot be null or white-space only", paramName: nameof(applicationName));
            }

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                throw new ArgumentException(message: "Environment name cannot be null or white-space only", paramName: nameof(environmentName));
            }

            this.applicationName = applicationName;
            this.environmentName = environmentName;
        }

        public static TestWebApplicationFactory Create(string applicationName, string environmentName)
        {
            return new TestWebApplicationFactory(applicationName, environmentName);
        }

        public static async Task<TestWebApplicationFactory> CreateAsync
        (
            string applicationName,
            string environmentName,
            bool shouldRunStartupLogicTasks = true
        )
        {
            TestWebApplicationFactory testWebApplicationFactory = Create(applicationName, environmentName);

            if (shouldRunStartupLogicTasks)
            {
                await testWebApplicationFactory
                    .Services
                    .GetRequiredService<IStartupLogicTaskExecutor>()
                    .ExecuteAsync()
                    .WaitAsync(timeout: RunStartupLogicTimeout);
            }

            return testWebApplicationFactory;
        }

        public async Task<HttpClient> CreateHttpClientAsync()
        {
            string accessToken = await GetAccessTokenAsync();
            HttpClient httpClient = CreateHttpClientWithLoggingCapabilities();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            return httpClient;
        }

        /// <summary>
        /// This method allows replacing registered services with mock implementations, as needed for testing purposes.
        /// </summary>
        /// <param name="setupMockServices">The action inside which registered services are replaced with mocks.</param>
        /// <returns>The current <see cref="TestWebApplicationFactory"/> instance.</returns>
        public TestWebApplicationFactory WithMockServices(Action<ContainerBuilder> setupMockServices)
        {
            setupMockServicesAction = setupMockServices;

            return this;
        }

        /// <summary>
        /// This method is part of a bigger workaround for a known issue, documented here:
        /// https://github.com/dotnet/aspnetcore/issues/14907#issuecomment-850407104.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected override IHost CreateHost(IHostBuilder builder)
        {
            if (setupMockServicesAction is not null)
            {
                builder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
                {
                    setupMockServicesAction.Invoke(containerBuilder);
                });
            }

            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environmentName);

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddConfiguration(GetDatabaseConfiguration());
            });

            builder.ConfigureTestServices(services =>
            {
                // Do not allow any hosted services, if any, to execute while running tests.
                services.RemoveAll(typeof(IHostedService));
            });
        }

        private IConfiguration GetDatabaseConfiguration()
        {
            string connectionString =
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables(prefix: EnvironmentVariables.Prefix)
                    .Build()
                    .GetConnectionString(name: environmentName);

            NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new(connectionString)
            {
                Database = $"db--{applicationName}",
                IncludeErrorDetail = true
            };

            IConfiguration databaseConfiguration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection
                    (
                        initialData: new List<KeyValuePair<string, string>>
                        {
                            new($"ConnectionStrings:{environmentName}", npgsqlConnectionStringBuilder.ConnectionString)
                        }
                    )
                    .Build();

            return databaseConfiguration;
        }

        private HttpClient CreateHttpClientWithLoggingCapabilities()
        {
            ILoggerFactory loggerFactory = Server.Services.GetService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<TestWebApplicationFactory>();
            HttpClient httpClient = CreateDefaultClient(new HttpLoggingHandler(new HttpClientHandler(), logger));

            return httpClient;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            GenerateJwtModel generateJwtModel = new()
            {
                UserName = $"user-{Guid.NewGuid():N}",
                Password = $"password-{Guid.NewGuid():N}"
            };

            HttpClient httpClient = CreateHttpClientWithLoggingCapabilities();

            using HttpResponseMessage httpResponseMessage =
                await httpClient.PostAsync
                (
                    requestUri: "api/jwt",
                    content: new StringContent(JsonSerializer.Serialize(generateJwtModel), Encoding.UTF8, "application/json")
                );

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                JwtModel jwtModel = await httpResponseMessage.Content.ReadFromJsonAsync<JwtModel>();
                return jwtModel.AccessToken;
            }

            throw new CouldNotGetJwtException(httpResponseMessage);
        }
    }
}
