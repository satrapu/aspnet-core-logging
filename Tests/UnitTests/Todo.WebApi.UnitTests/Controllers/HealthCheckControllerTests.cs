namespace Todo.WebApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using VerifyNUnit;

    /// <summary>
    /// Contains unit tests targeting <see cref="HealthCheckController" /> class.
    /// </summary>
    [TestFixture]
    public class HealthCheckControllerTests
    {
        [Test]
        public async Task GetHealthReportAsync_WhenTimeoutExceptionOccurs_ReturnsExpectedHealthReport()
        {
            // Arrange
            HealthCheckServiceThrowingExceptions healthCheckServiceThrowingExceptions = new
            (
                exceptionToThrow: new TimeoutException
                (
                    message: "This is a hard-coded timeout exception created for testing purposes"
                )
            );

            HealthCheckController classUnderTest = new(healthCheckService: healthCheckServiceThrowingExceptions);

            // Act
            ActionResult actionResult = await classUnderTest.GetHealthReportAsync(cancellationToken: CancellationToken.None);

            // Assert
            await Verifier.Verify(actionResult, settings: ModuleInitializer.VerifySettings);
        }

        [Test]
        public async Task GetHealthReportAsync_WhenUnexpectedExceptionOccurs_ReturnsExpectedHealthReport()
        {
            // Arrange
            HealthCheckServiceThrowingExceptions healthCheckServiceThrowingExceptions = new
            (
                exceptionToThrow: new Exception(message: "This is a hard-coded exception created for testing purposes")
            );

            HealthCheckController classUnderTest = new(healthCheckService: healthCheckServiceThrowingExceptions);

            // Act
            ActionResult actionResult = await classUnderTest.GetHealthReportAsync(cancellationToken: CancellationToken.None);

            // Assert
            await Verifier.Verify(actionResult, settings: ModuleInitializer.VerifySettings);
        }

        private class HealthCheckServiceThrowingExceptions : HealthCheckService
        {
            private readonly Exception exceptionToThrow;

            public HealthCheckServiceThrowingExceptions(Exception exceptionToThrow)
            {
                this.exceptionToThrow = exceptionToThrow;
            }

            public override Task<HealthReport> CheckHealthAsync(Func<HealthCheckRegistration, bool> predicate, CancellationToken cancellationToken = default)
            {
                throw exceptionToThrow;
            }
        }
    }
}
