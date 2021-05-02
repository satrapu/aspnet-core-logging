namespace Todo.ApplicationFlows.Security
{
    using System;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    using Services.Security;

    /// <summary>
    /// Contains unit tests targeting <seealso cref="GenerateJwtFlow"/> class.
    /// </summary>
    [TestFixture]
    public class GenerateJwtFlowTests
    {
        [Test]
        public void Constructor_WhenJwtServiceIsNull_ThrowsException()
        {
            // Arrange
            IJwtService jwtService = null;
            var logger = new Mock<ILogger<GenerateJwtFlow>>();

            // Act
            // ReSharper disable once CA1806
            // ReSharper disable once ExpressionIsAlwaysNull
            Action constructorCall = () =>  new GenerateJwtFlow(jwtService, logger.Object);

            // Assert
            constructorCall
                .Should().ThrowExactly<ArgumentNullException>("because service is null")
                .And.ParamName.Should().Be(nameof(jwtService));
        }

        [Test]
        public async Task ExecuteAsync_WhenGenerateJwtInfoIsValid_ReturnsExpectedData()
        {
            // Arrange
            var expectedJwtInfo = new JwtInfo();
            var jwtService = new Mock<IJwtService>();
            jwtService.Setup(x => x.GenerateJwtAsync(It.IsAny<GenerateJwtInfo>())).ReturnsAsync(expectedJwtInfo);

            var logger = new Mock<ILogger<GenerateJwtFlow>>();
            var generateJwtInfo = new GenerateJwtInfo();

            var principal = new Mock<IPrincipal>();
            principal.SetupGet(x => x.Identity).Returns(new GenericIdentity("test"));

            var generateJwtFlow = new GenerateJwtFlow(jwtService.Object, logger.Object);

            // Act
            var actualJwtInfo = await generateJwtFlow.ExecuteAsync(generateJwtInfo, principal.Object);

            // Assert
            actualJwtInfo.Should().BeEquivalentTo(expectedJwtInfo, $"because {nameof(GenerateJwtFlow)} works");
        }
    }
}
