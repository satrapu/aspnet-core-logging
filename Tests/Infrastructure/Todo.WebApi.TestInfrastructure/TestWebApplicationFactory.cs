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

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Models;

    using Npgsql;

    using WebApi;

    /// <summary>
    /// A <see cref="WebApplicationFactory{TEntryPoint}"/> implementation to be used for running integration tests.
    /// <br/>
    /// Each instance of this class will use its own database to ensure isolation at test class level.
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private const string ConnectionStringKey = "ConnectionStrings:TodoForIntegrationTests";
        private Action<ContainerBuilder> setupMockServicesAction;
        private readonly string applicationName;

        public TestWebApplicationFactory(string applicationName)
        {
            this.applicationName = applicationName;
        }

        public async Task<HttpClient> CreateHttpClientWithJwtAsync()
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
            builder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                setupMockServicesAction?.Invoke(containerBuilder);
            });

            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            string environmentName = EnvironmentNames.IntegrationTests;
            builder.UseEnvironment(environmentName);

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                IConfiguration configuration = CreateConfigurationForEnvironment(environmentName);
                configurationBuilder.AddConfiguration(configuration);
            });

            builder.ConfigureTestServices(services =>
            {
                // Do not allow any hosted services, if any, to execute while running tests.
                services.RemoveAll(typeof(IHostedService));
            });
        }

        private IConfiguration CreateConfigurationForEnvironment(string environmentName)
        {
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(connectionString: configuration.GetValue<string>(ConnectionStringKey))
            {
                Database = $"db4it--{applicationName}",
                IncludeErrorDetail = true
            };

            MemoryConfigurationSource memoryConfigurationSource = new()
            {
                InitialData = new List<KeyValuePair<string, string>>
                {
                    new(ConnectionStringKey, connectionStringBuilder.ConnectionString)
                }
            };

            IConfiguration enhancedConfiguration =
                new ConfigurationBuilder()
                    .AddConfiguration(configuration)
                    .Add(memoryConfigurationSource)
                    .Build();

            return enhancedConfiguration;
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
