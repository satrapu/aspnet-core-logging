namespace Todo.WebApi.Controllers
{
    using System;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Controller used for dealing with application configuration.
    /// </summary>
    [Route("api/configuration")]
    [Authorize]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationRoot configurationRoot;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ConfigurationController(IConfiguration applicationConfiguration, IWebHostEnvironment webHostEnvironment)
        {
            configurationRoot = applicationConfiguration as IConfigurationRoot ??
                                throw new ArgumentException("Expected configuration root",
                                    nameof(applicationConfiguration));

            this.webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }


        /// <summary>
        /// Display each application configuration property, along with its source.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetConfigurationDebugView()
        {
            bool isDebugViewEnabled = configurationRoot.GetValue<bool>("ConfigurationDebugViewEndpointEnabled");

            if (!webHostEnvironment.IsDevelopment() || !isDebugViewEnabled)
            {
                return Forbid();
            }

            return new ObjectResult(configurationRoot.GetDebugView());
        }
    }
}
