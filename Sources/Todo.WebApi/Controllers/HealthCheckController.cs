namespace Todo.WebApi.Controllers
{
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/health")]
    [AllowAnonymous]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly HealthCheckService healthCheckService;

        public HealthCheckController(HealthCheckService healthCheckService)
        {
            this.healthCheckService = healthCheckService;
        }

        [HttpGet]
        public async Task<ActionResult> GetHealthReportAsync()
        {
            HealthReport healthReport = await healthCheckService.CheckHealthAsync();

            HttpStatusCode httpStatusCode = healthReport.Status switch
            {
                HealthStatus.Degraded => HttpStatusCode.OK,
                HealthStatus.Healthy => HttpStatusCode.OK,
                _ => HttpStatusCode.ServiceUnavailable
            };

            return StatusCode(statusCode: (int)httpStatusCode, value: healthReport);
        }
    }
}
