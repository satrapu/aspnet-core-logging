namespace Todo.WebApi.Controllers
{
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Commons.Constants;

    using NUnit.Framework;

    using TestInfrastructure;

    using VerifyNUnit;

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

            activityListener = new ActivityListener()
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = _ => { },
                ActivityStopped = _ => { }
            };

            noOpActivityListener = new ActivityListener()
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
            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync(BaseUrl);

            // Assert
            await Verifier.Verify(response, settings: ModuleInitializer.VerifySettings);
        }

        [Test]
        public async Task GetHealthReportAsync_WhenRequestIsNotAuthorized_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync(BaseUrl);

            // Assert
            await Verifier.Verify(response, settings: ModuleInitializer.VerifySettings);
        }
    }
}
