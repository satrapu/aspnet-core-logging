namespace Todo.Services.Security
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// An <see cref="IJwtService"/> implementation.
    /// <br/>
    /// Based on: https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/.
    /// </summary>
    public class JwtService : IJwtService
    {
        public async Task<JwtInfo> GenerateJwtAsync(GenerateJwtInfo generateJwtInfo)
        {
            byte[] userNameAsBytes = Encoding.UTF8.GetBytes(generateJwtInfo.UserName);
            string userNameAsBase64 = Convert.ToBase64String(userNameAsBytes);
            SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(generateJwtInfo.Secret));

            SecurityTokenDescriptor securityTokenDescriptor = new()
            {
                Audience = generateJwtInfo.Audience,
                Issuer = generateJwtInfo.Issuer,
                Expires = DateTime.UtcNow.AddMonths(6),
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature),
                Subject = new ClaimsIdentity
                (
                    claims:
                    [
                        new Claim(ClaimTypes.NameIdentifier, userNameAsBase64),
                        new Claim("scope", string.Join(separator: ' ', generateJwtInfo.Scopes ?? []))
                    ]
                )
            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

            JwtInfo jwtInfo = new()
            {
                AccessToken = jwtSecurityTokenHandler.WriteToken(securityToken)
            };

            return await Task.FromResult(jwtInfo);
        }
    }
}
