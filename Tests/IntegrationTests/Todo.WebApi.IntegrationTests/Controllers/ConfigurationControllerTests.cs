namespace Todo.WebApi.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Commons.Constants;

    using FluentAssertions;

    using NUnit.Framework;

    using TestInfrastructure;

    /// <summary>
    /// Contains integration tests targeting <see cref="ConfigurationController" /> class.
    /// </summary>
    [TestFixture]
    public class ConfigurationControllerTests
    {
        [Test]
        [TestCaseSource(nameof(GetConfigurationEndpointContext))]
        public async Task GetConfigurationDebugView_WhenCalled_MustBehaveAsExpected(string environmentName, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            await using TestWebApplicationFactory testWebApplicationFactory = await TestWebApplicationFactory.CreateAsync
            (
                applicationName: nameof(ConfigurationControllerTests),
                environmentName: environmentName
            );

            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync("api/configuration");

            // Assert
            response.StatusCode.Should().Be(expectedStatusCode, "because configuration endpoint is available only in certain conditions");
        }

        private static IEnumerable<object[]> GetConfigurationEndpointContext()
        {
            yield return new object[]
            {
                EnvironmentNames.Development, HttpStatusCode.OK
            };

            yield return new object[]
            {
                EnvironmentNames.IntegrationTests, HttpStatusCode.Forbidden
            };

            yield return new object[]
            {
                EnvironmentNames.AcceptanceTests, HttpStatusCode.Forbidden
            };

            yield return new object[]
            {
                EnvironmentNames.DemoInAzure, HttpStatusCode.Forbidden
            };

            yield return new object[]
            {
                EnvironmentNames.Staging, HttpStatusCode.Forbidden
            };

            yield return new object[]
            {
                EnvironmentNames.Production, HttpStatusCode.Forbidden
            };
        }
    }
}
