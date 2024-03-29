namespace Todo.WebApi.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Commons.Constants;

    using FluentAssertions;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;

    using NUnit.Framework;

    /// <summary>
    /// Contains integration tests targeting <see cref="ConfigurationController" /> class.
    /// </summary>
    [TestFixture]
    public class ConfigurationControllerTests
    {
        [Test]
        [TestCaseSource(nameof(GetConfigurationEndpointContext))]
        public async Task GetConfigurationDebugView_WhenCalled_MustBehaveAsExpected(string environmentName,
            bool isDebugViewEnabled, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            using var webApplicationFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseEnvironment(environmentName);

                    webHostBuilder.ConfigureAppConfiguration((_, configurationBuilder) =>
                    {
                        configurationBuilder.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string>("ConfigurationDebugViewEndpointEnabled",
                                isDebugViewEnabled.ToString()),

                            // Ensure database is not migrated while running tests found in this test class, no matter
                            // the environment specified in this method.
                            new KeyValuePair<string, string>("MigrateDatabase", bool.FalseString),

                            // Ensure OpenTelemetry is not enabled while running tests found in this test class, no matter
                            // the environment specified in this method.
                            new KeyValuePair<string, string>("OpenTelemetry:Enabled", bool.FalseString)
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

        private static IEnumerable<object[]> GetConfigurationEndpointContext()
        {
            yield return new object[] { EnvironmentNames.Development, false, HttpStatusCode.Forbidden };
            yield return new object[] { EnvironmentNames.Development, true, HttpStatusCode.OK };
            yield return new object[] { EnvironmentNames.IntegrationTests, true, HttpStatusCode.Forbidden };
            yield return new object[] { EnvironmentNames.IntegrationTests, false, HttpStatusCode.Forbidden };
            yield return new object[] { EnvironmentNames.DemoInAzure, true, HttpStatusCode.Forbidden };
            yield return new object[] { EnvironmentNames.DemoInAzure, false, HttpStatusCode.Forbidden };
            yield return new object[] { EnvironmentNames.Production, true, HttpStatusCode.Forbidden };
            yield return new object[] { EnvironmentNames.Production, false, HttpStatusCode.Forbidden };
        }
    }
}
