namespace Todo.WebApi.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Commons.Constants;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Newtonsoft.Json.Linq;

    using NUnit.Framework;

    using TestInfrastructure;

    /// <summary>
    /// Contains integration tests targeting <see cref="HealthCheckController" /> class.
    /// </summary>
    [TestFixture]
    public class HealthCheckControllerTests
    {
        private const string BaseUrl = "api/health";

        private TestWebApplicationFactory testWebApplicationFactory;
        private ActivityListener activityListener;
        private ActivityListener noOpActivityListener;

        /// <summary>
        /// Ensures the appropriate <see cref="TestWebApplicationFactory"/> instance has been created before running
        /// any test method found in this class.
        /// </summary>
        [OneTimeSetUp]
        public async Task GivenAnHttpRequestIsToBePerformed()
        {
            testWebApplicationFactory = await TestWebApplicationFactory.CreateAsync
            (
                applicationName: nameof(TodoControllerTests),
                environmentName: EnvironmentNames.IntegrationTests,
                shouldRunStartupLogicTasks: true
            );

            activityListener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = _ => { },
                ActivityStopped = _ => { }
            };

            noOpActivityListener = new ActivityListener
            {
                ShouldListenTo = _ => false,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.None,
                ActivityStarted = _ => { },
                ActivityStopped = _ => { }
            };

            ActivitySource.AddActivityListener(activityListener);
            ActivitySource.AddActivityListener(noOpActivityListener);
        }

        /// <summary>
        /// Ensure the <see cref="TestWebApplicationFactory"/> instance is properly disposed after all test methods
        /// found inside this class have been run.
        /// </summary>
        [OneTimeTearDown]
        public void Cleanup()
        {
            testWebApplicationFactory?.Dispose();
        }

        [Test]
        public async Task GetHealthReportAsync_UsingValidInput_ReturnsExpectedHealthReport()
        {
            // Arrange
            TimeSpan maxWaitTimeForHealthChecks = HealthCheckController.MaxHealthCheckDuration;

            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync(BaseUrl);

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JObject responseContent = await response.Content.ReadAsAsync<JObject>();
            responseContent.Should().NotBeNull();

            JObject healthReport = responseContent.Value<JObject>(key: "healthReport");
            healthReport.Should().NotBeNull();

            string status = healthReport.Value<string>(key: "status");
            status.Should().Be("Healthy");

            string description = healthReport.Value<string>(key: "description");
            description.Should().Be("All dependencies have been successfully checked");

            string duration = healthReport.Value<string>(key: "duration");
            duration.As<TimeSpan>().Should().BeLessOrEqualTo(maxWaitTimeForHealthChecks);

            JArray dependencies = healthReport.Value<JArray>("dependencies");
            dependencies.Should().NotBeNullOrEmpty();

            JToken persistentStorage = dependencies.Children().FirstOrDefault(child => child.Value<string>(key: "name") is "Persistent storage");
            persistentStorage.Should().NotBeNull();
            persistentStorage!.Value<string>(key: "status").Should().Be("Healthy");
            persistentStorage.Value<string>(key: "duration").As<TimeSpan>().Should().BeLessOrEqualTo(maxWaitTimeForHealthChecks);
        }

        [Test]
        public async Task GetHealthReportAsync_WhenRequestIsNotAuthorized_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync(BaseUrl);

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
