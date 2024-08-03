namespace Todo.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/health")]
    [AllowAnonymous]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private static readonly TimeSpan MaxWaitTimeForHealthChecks = TimeSpan.FromSeconds(2);
        private readonly HealthCheckService healthCheckService;

        public HealthCheckController(HealthCheckService healthCheckService)
        {
            this.healthCheckService = healthCheckService;
        }

        [HttpGet]
        public async Task<ActionResult> GetHealthReportAsync(CancellationToken cancellationToken)
        {
            TimeSpan maxWaitTimeForHealthChecks = MaxWaitTimeForHealthChecks;
            Exception checkHealthException = null;
            HealthReport healthReport;

            try
            {
                healthReport =
                    await healthCheckService
                        .CheckHealthAsync(cancellationToken)
                        .WaitAsync(timeout: maxWaitTimeForHealthChecks);
            }
            catch (Exception exception)
            {
                checkHealthException = exception;

                healthReport = new HealthReport
                (
                    entries: new Dictionary<string, HealthReportEntry>(),
                    status: HealthStatus.Unhealthy,
                    totalDuration: maxWaitTimeForHealthChecks
                );
            }

            return StatusCode
            (
                statusCode: (int)GetHttpStatusCode(healthReport),
                value: GetProjectedHealthReport(healthReport, checkHealthException)
            );
        }

        private static dynamic GetProjectedHealthReport(HealthReport healthReport, Exception checkHealthException = null)
        {
            return new
            {
                HealthReport = new
                {
                    Status = healthReport.Status.ToString("G"),
                    Description =
                        checkHealthException is null
                            ? "All dependencies have been successfully checked"
                            : GetUserFriendlyDescription(checkHealthException),
                    Duration = healthReport.TotalDuration.ToString("g"),
                    Dependencies = healthReport.Entries.Select(healthReportEntry => new
                    {
                        Name = healthReportEntry.Key,
                        Status = healthReportEntry.Value.Status.ToString("G"),
                        Duration = healthReportEntry.Value.Duration.ToString("g")
                    })
                }
            };
        }

        private static HttpStatusCode GetHttpStatusCode(HealthReport healthReport)
        {
            return healthReport.Status switch
            {
                HealthStatus.Degraded => HttpStatusCode.OK,
                HealthStatus.Healthy => HttpStatusCode.OK,
                _ => HttpStatusCode.ServiceUnavailable
            };
        }

        private static string GetUserFriendlyDescription(Exception exception)
        {
            return exception switch
            {
                TimeoutException _ => "Failed to check dependencies due to a timeout",
                _ => "Failed to check dependencies due to an unexpected error"
            };
        }
    }
}
