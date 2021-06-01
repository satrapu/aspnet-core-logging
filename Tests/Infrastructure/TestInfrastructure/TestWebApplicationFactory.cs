namespace Todo.TestInfrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Principal;
    using System.Text;
    using System.Threading.Tasks;

    using ApplicationFlows.Security;

    using Autofac;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using Npgsql;

    using Services.Security;

    using WebApi;
    using WebApi.Configuration;
    using WebApi.Models;

    /// <summary>
    /// A <see cref="WebApplicationFactory{TEntryPoint}"/> implementation to be used for running integration tests.
    /// <br/>
    /// Each instance of this class will use its own database to ensure isolation at test class level.
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private const string ConnectionStringKey = "ConnectionStrings:TodoForIntegrationTests";
        private readonly string applicationName;

        public TestWebApplicationFactory(string applicationName)
        {
            this.applicationName = applicationName;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            const string environmentName = WebHostEnvironmentExtensions.IntegrationTestsEnvironmentName;
            builder.UseEnvironment(environmentName);

            builder.ConfigureAppConfiguration((webHostBuilderContext, configurationBuilder) =>
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

        public async Task<HttpClient> CreateClientWithJwtAsync()
        {
            string accessToken = await GetAccessTokenAsync();
            HttpClient httpClient = CreateClientWithLoggingCapabilities();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            return httpClient;
        }

        private IConfiguration CreateConfigurationForEnvironment(string environmentName)
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionStringBuilder =
                new NpgsqlConnectionStringBuilder(configurationRoot.GetValue<string>(ConnectionStringKey))
                {
                    Database = $"db4it--{applicationName}"
                };

            var memoryConfigurationSource = new MemoryConfigurationSource
            {
                InitialData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(ConnectionStringKey, connectionStringBuilder.ConnectionString)
                }
            };

            IConfigurationRoot enhancedConfigurationRoot = new ConfigurationBuilder()
                .AddConfiguration(configurationRoot)
                .Add(memoryConfigurationSource)
                .Build();

            return enhancedConfigurationRoot;
        }

        private HttpClient CreateClientWithLoggingCapabilities()
        {
            ILoggerFactory loggerFactory = Server.Services.GetService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<TestWebApplicationFactory>();
            HttpClient httpClient = CreateDefaultClient(new HttpLoggingHandler(new HttpClientHandler(), logger));

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
    }
}
