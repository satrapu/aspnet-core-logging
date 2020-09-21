using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Todo.WebApi.Authorization;
using Todo.WebApi.Models;

namespace Todo.WebApi.Controllers
{
    /// <summary>
    /// Creates JSON web tokens to be used when authentication and authorize users accessing this web API.
    /// Based on: https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/.
    /// WARNING: This controller is *not* ready for production! It has been written for experimenting purposes only,
    /// so the logic of generating a JSON web token for a given user name and password has been greatly simplified.
    /// In the future, a better mechanism will be implemented.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class JwtController : ControllerBase
    {
        private readonly GenerateJwtOptions generateJwtOptions;

        public JwtController(IOptionsMonitor<GenerateJwtOptions> generateJwtOptionsMonitor)
        {
            // Options pattern: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1.
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
            JwtModel jwtModel = await Task
                .FromResult(GenerateToken(generateJwtModel.UserName, generateJwtModel.Password))
                .ConfigureAwait(false);

            return Ok(jwtModel);
        }

        // ReSharper disable once UnusedParameter.Local
        private JwtModel GenerateToken(string userName, string password)
        {
            byte[] userNameAsBytes = Encoding.UTF8.GetBytes(userName);
            string userNameAsBase64 = Convert.ToBase64String(userNameAsBytes);
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(generateJwtOptions.Secret));

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userNameAsBase64),
                    new Claim("scope", string.Join(' '
                                                           , Policies.TodoItems.CreateTodoItem
                                                           , Policies.TodoItems.DeleteTodoItem
                                                           , Policies.TodoItems.GetTodoItems
                                                           , Policies.TodoItems.UpdateTodoItem))
                }),
                Expires = DateTime.UtcNow.AddMonths(6),
                Issuer = generateJwtOptions.Issuer,
                Audience = generateJwtOptions.Audience,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

            var jwtModel = new JwtModel
            {
                AccessToken = jwtSecurityTokenHandler.WriteToken(securityToken),
            };

            return jwtModel;
        }
    }
}