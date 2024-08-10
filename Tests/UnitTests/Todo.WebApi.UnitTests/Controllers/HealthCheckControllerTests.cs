using System.Net;

namespace Todo.WebApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using NUnit.Framework;

    /// <summary>
    /// Contains integration tests targeting <see cref="HealthCheckController" /> class.
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
            using AssertionScope _ = new();
            actionResult.Should().NotBeNull();

            ObjectResult objectResult = actionResult.As<ObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
            objectResult.Value.Should().NotBeNull();
            objectResult.Value.Should().BeEquivalentTo
            (
                expectation: new
                {
                    HealthReport = new
                    {
                        Status = "Unhealthy",
                        Description = "Failed to check dependencies due to a timeout",
                        Duration = HealthCheckController.MaxHealthCheckDuration.ToString("g"),
                        Dependencies = Array.Empty<object>()
                    }
                }
            );
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
            using AssertionScope _ = new();
            actionResult.Should().NotBeNull();

            ObjectResult objectResult = actionResult.As<ObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
            objectResult.Value.Should().NotBeNull();
            objectResult.Value.Should().BeEquivalentTo
            (
                expectation: new
                {
                    HealthReport = new
                    {
                        Status = "Unhealthy",
                        Description = "Failed to check dependencies due to an unexpected error",
                        Duration = HealthCheckController.MaxHealthCheckDuration.ToString("g"),
                        Dependencies = Array.Empty<object>()
                    }
                }
            );
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
