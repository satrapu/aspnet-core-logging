using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Todo.WebApi.Models;

namespace Todo.WebApi.Controllers
{
    /// <summary>
    /// Creates JWT tokens to be used when authentication and authorize users accessing this web API.
    /// Based on: https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/.
    /// WARNING: This controller is *not* ready for production! It has been written for experimenting purposes only,
    /// so the logic of generating a JWT token for a given user name and password has been greatly simplified.
    /// In the future, a better mechanism will be implemented.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class JwtTokensController : ControllerBase
    {
        private readonly GenerateJwtTokensOptions generateJwtTokensOptions;

        public JwtTokensController(IOptionsMonitor<GenerateJwtTokensOptions> generateJwtTokensOptionsMonitor)
        {
            // Options pattern: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1.
            generateJwtTokensOptions = generateJwtTokensOptionsMonitor.CurrentValue;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateJwtTokenAsync([FromBody] GenerateJwtTokenModel generateJwtTokenModel)
        {
            JwtTokenModel jwtTokenModel = await Task
                .FromResult(GenerateJwtToken(generateJwtTokenModel.UserName, generateJwtTokenModel.Password))
                .ConfigureAwait(false);

            return Ok(jwtTokenModel);
        }

        private JwtTokenModel GenerateJwtToken(string userName, string password)
        {
            var symmetricSecurityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(generateJwtTokensOptions.Secret));

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString("N")),
                    new Claim("scope", string.Join(' ', "get:todo", "create:todo", "update:todo", "delete:todo")),
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = generateJwtTokensOptions.Issuer,
                Audience = generateJwtTokensOptions.Audience,
                SigningCredentials =
                    new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

            var generatedJwtTokenModel = new JwtTokenModel
            {
                AccessToken = jwtSecurityTokenHandler.WriteToken(securityToken),
            };

            return generatedJwtTokenModel;
        }
    }
}