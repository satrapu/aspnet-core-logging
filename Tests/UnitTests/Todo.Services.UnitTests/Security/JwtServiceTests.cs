namespace Todo.Services.Security
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using NUnit.Framework;

    /// <summary>
    /// Contains unit tests targeting <see cref="JwtService"/> class.
    /// </summary>
    [TestFixture]
    public class JwtServiceTests
    {
        [Test]
        public async Task GenerateJwtAsync_WhenUsingValidInput_MustReturnExpectedResult()
        {
            // Arrange
            GenerateJwtInfo generateJwtInfo = new()
            {
                UserName = "some-test-user",
                Password = "some-password",
                Scopes = new[] { "resource1", "resource2" },
                Audience = "test-audience",
                Issuer = "test",
                Secret = "!z%*mEPs>_[9`MZ\"P:@rm%#zYnGA=HOn<RS=j\"vO9$,cvhh2zd"
            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            JwtService jwtService = new JwtService();

            // Act
            JwtInfo actualResult = await jwtService.GenerateJwtAsync(generateJwtInfo);

            // Assert
            using AssertionScope _ = new();
            actualResult.Should().NotBeNull();
            actualResult.AccessToken.Should().NotBeNullOrWhiteSpace();

            JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(actualResult.AccessToken);
            jwtSecurityToken.Should().NotBeNull();
            jwtSecurityToken.Audiences.Should().ContainSingle(generateJwtInfo.Audience);
            jwtSecurityToken.Issuer.Should().Be(generateJwtInfo.Issuer);
            jwtSecurityToken.Claims.Should().Contain(claim => claim.Type == "scope" && claim.Value == "resource1 resource2");
            jwtSecurityToken.ValidTo.Should().BeCloseTo(nearbyTime: jwtSecurityToken.ValidFrom.AddMonths(6), precision: TimeSpan.FromSeconds(1));
        }
    }
}
