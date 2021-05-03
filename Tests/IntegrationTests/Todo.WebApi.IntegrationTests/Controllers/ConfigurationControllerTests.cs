namespace Todo.WebApi.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Hosting;

    using NUnit.Framework;

    /// <summary>
    /// Contains integration tests targeting <seealso cref="ConfigurationController" /> class.
    /// </summary>
    [TestFixture]
    public class ConfigurationControllerTests
    {
        [Test]
        public async Task GetConfigurationDebugView_WhenCalledInsideDevelopmentEnvironment_MustSucceeds()
        {
            // Arrange
            var webApplicationFactory = new WebApplicationFactory<Startup>().WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseEnvironment(Environments.Development);
            });

            try
            {
                using HttpClient httpClient = webApplicationFactory.CreateClient();

                // Act
                HttpResponseMessage response = await httpClient.GetAsync("api/configuration");

                // Assert
                string because =
                    $"because configuration endpoint must be available in {Environments.Development} environment";

                using var assertionScope = new AssertionScope();
                response.StatusCode.Should().Be(HttpStatusCode.OK, because);

                string content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrWhiteSpace(because);
            }
            finally
            {
                webApplicationFactory.Dispose();
            }
        }

        [Test]
        [TestCaseSource(nameof(environmentNamesWhereConfigurationDebugViewIsDisabled))]
        public async Task GetConfigurationDebugView_WhenCalledOutsideDevelopmentEnvironment_MustFail(
            string environmentName)
        {
            var webApplicationFactory = new WebApplicationFactory<Startup>().WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseEnvironment(environmentName);
            });

            try
            {
                // Arrange
                using HttpClient httpClient = webApplicationFactory.CreateClient();

                // Act
                HttpResponseMessage response = await httpClient.GetAsync("api/configuration");

                // Assert
                string because =
                    $"because configuration endpoint must *not* be available outside {Environments.Development} environment";

                using var assertionScope = new AssertionScope();
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden, because);

                string content = await response.Content.ReadAsStringAsync();
                content.Should().BeEmpty(because);
            }
            finally
            {
                webApplicationFactory.Dispose();
            }
        }

        private static object[] environmentNamesWhereConfigurationDebugViewIsDisabled =
        {
            new object[] {"IntegrationTests"},
            new object[] {"DemoInAzure"},
            new object[] {"Production"},
        };
    }
}
