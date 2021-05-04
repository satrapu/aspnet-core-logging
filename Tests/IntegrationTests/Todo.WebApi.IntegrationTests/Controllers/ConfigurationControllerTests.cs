namespace Todo.WebApi.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    using NUnit.Framework;

    /// <summary>
    /// Contains integration tests targeting <seealso cref="ConfigurationController" /> class.
    /// </summary>
    [TestFixture]
    public class ConfigurationControllerTests
    {
        [Test]
        [TestCaseSource(nameof(configurationEndpointContext))]
        public async Task GetConfigurationDebugView_WhenCalled_MustBehaveAsExpected(string environment,
            bool isDebugViewEnabled, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            using var webApplicationFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseEnvironment(environment);

                    webHostBuilder.ConfigureAppConfiguration((webHostBuilderContext, configurationBuilder) =>
                    {
                        configurationBuilder.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string>("ConfigurationDebugViewEndpointEnabled",
                                isDebugViewEnabled.ToString())
                        });
                    });
                });

            using HttpClient httpClient = webApplicationFactory.CreateClient();

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync("api/configuration");

            // Assert
            response.StatusCode.Should().Be(expectedStatusCode,
                "because configuration endpoint is available only in certain conditions");
        }

        private static object[] configurationEndpointContext =
        {
            new object[] {Environments.Development, false, HttpStatusCode.Forbidden},
            new object[] {Environments.Development, true, HttpStatusCode.OK},
            new object[] {"IntegrationTests", true, HttpStatusCode.Forbidden},
            new object[] {"IntegrationTests", false, HttpStatusCode.Forbidden},
            new object[] {"DemoInAzure", true, HttpStatusCode.Forbidden},
            new object[] {"DemoInAzure", false, HttpStatusCode.Forbidden},
            new object[] {Environments.Production, true, HttpStatusCode.Forbidden},
            new object[] {Environments.Production, false, HttpStatusCode.Forbidden}
        };
    }
}
