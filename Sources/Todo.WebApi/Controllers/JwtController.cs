namespace Todo.WebApi.Controllers
{
    using System;
    using System.Threading.Tasks;

    using ApplicationFlows.Security;

    using Authorization;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    using Models;

    using Services.Security;

    /// <summary>
    /// Creates JSON web tokens to be used by the users of this web API for authentication and authorization purposes.
    /// </summary>
    /// <remarks>
    /// This controller is *not* ready for production! It has been written for experimenting purposes only,
    /// so the logic of generating a JSON web token for a given user name and password has been greatly simplified.
    /// In the future, a better mechanism will be implemented.
    /// </remarks>
    [Route("api/jwt")]
    [Authorize]
    [ApiController]
    public class JwtController : ControllerBase
    {
        private readonly IGenerateJwtFlow generateJwtFlow;
        private readonly GenerateJwtOptions generateJwtOptions;

        public JwtController(IGenerateJwtFlow generateJwtFlow,
            IOptionsMonitor<GenerateJwtOptions> generateJwtOptionsMonitor)
        {
            this.generateJwtFlow = generateJwtFlow ?? throw new ArgumentNullException(nameof(generateJwtFlow));

            // Options pattern: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0.
            generateJwtOptions = generateJwtOptionsMonitor.CurrentValue;
        }

        /// <summary>
        /// Generates a JSON web token.
        /// </summary>
        /// <param name="generateJwtModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateTokenAsync([FromBody] GenerateJwtModel generateJwtModel)
        {
            var generateJwtInfo = new GenerateJwtInfo
            {
                Audience = generateJwtOptions.Audience,
                Issuer = generateJwtOptions.Issuer,
                Secret = generateJwtOptions.Secret,
                Scopes = new[]
                {
                    Policies.TodoItems.CreateTodoItem,
                    Policies.TodoItems.DeleteTodoItem,
                    Policies.TodoItems.GetTodoItems,
                    Policies.TodoItems.UpdateTodoItem
                },
                UserName = generateJwtModel.UserName,
                Password = generateJwtModel.Password
            };

            JwtInfo jwtInfo = await generateJwtFlow.ExecuteAsync(generateJwtInfo, User);

            var jwtModel = new JwtModel
            {
                AccessToken = jwtInfo.AccessToken
            };

            return Ok(jwtModel);
        }
    }
}
