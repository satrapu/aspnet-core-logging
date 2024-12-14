namespace Todo.Services.Security
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using NUnit.Framework;

    using VerifyNUnit;

    using VerifyTests;

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
            VerifySettings verifySettings = new(ModuleInitializer.VerifySettings);
            verifySettings.ScrubMember("nbf");
            verifySettings.ScrubMember("exp");
            verifySettings.ScrubMember("iat");
            verifySettings.ScrubMember("EncodedPayload");
            verifySettings.ScrubMember("RawData");
            verifySettings.ScrubMember("RawPayload");
            verifySettings.ScrubMember("RawSignature");

            GenerateJwtInfo generateJwtInfo = new()
            {
                UserName = "some-test-user",
                Password = "some-password",
                Scopes = ["resource1", "resource2"],
                Audience = "test-audience",
                Issuer = "test",
                Secret = "!z%*mEPs>_[9`MZ\"P:@rm%#zYnGA=HOn<RS=j\"vO9$,cvhh2zd"
            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            JwtService classUnderTest = new();

            // Act
            JwtInfo actualResult = await classUnderTest.GenerateJwtAsync(generateJwtInfo);

            // Assert
            using AssertionScope _ = new();
            actualResult.Should().NotBeNull();
            actualResult.AccessToken.Should().NotBeNullOrWhiteSpace();

            JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(actualResult.AccessToken);
            await Verifier.Verify(jwtSecurityToken, settings: verifySettings);
        }
    }
}
