namespace Todo.ApplicationFlows.Security
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    using Services.Security;

    /// <summary>
    /// Contains unit tests targeting <see cref="GenerateJwtFlow"/> class.
    /// </summary>
    [TestFixture]
    public class GenerateJwtFlowTests
    {
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1806:Do not ignore method results",
            Justification = "Newly created instance is discarded for testing purposes")]
        public void Constructor_WhenJwtServiceIsNull_ThrowsException()
        {
            // Arrange
            IJwtService jwtService = null;
            Mock<ILogger<GenerateJwtFlow>> logger = new();

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once ExpressionIsAlwaysNull
            Action constructorCall = () => new GenerateJwtFlow(jwtService, logger.Object);

            // Assert
            constructorCall
                .Should().ThrowExactly<ArgumentNullException>("because service is null")
                .And.ParamName.Should().Be(nameof(jwtService));
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1806:Do not ignore method results",
            Justification = "Newly created instance is discarded for testing purposes")]
        public void Constructor_WhenLoggerIsNull_ThrowsException()
        {
            // Arrange
            Mock<IJwtService> jwtService = new();
            ILogger<GenerateJwtFlow> logger = null;

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once ExpressionIsAlwaysNull
            Action constructorCall = () => new GenerateJwtFlow(jwtService.Object, logger);

            // Assert
            constructorCall
                .Should().ThrowExactly<ArgumentNullException>("because logger is null")
                .And.ParamName.Should().Be(nameof(logger));
        }

        [Test]
        public async Task ExecuteAsync_WhenGenerateJwtInfoIsValidReturnsExpectedResult()
        {
            // Arrange
            JwtInfo expectedJwtInfo = new();

            Mock<IJwtService> jwtService = new();
            jwtService.Setup(x => x.GenerateJwtAsync(It.IsAny<GenerateJwtInfo>())).ReturnsAsync(expectedJwtInfo);

            Mock<ILogger<GenerateJwtFlow>> logger = new();
            GenerateJwtInfo generateJwtInfo = new();

            Mock<IPrincipal> principal = new();
            principal.SetupGet(x => x.Identity).Returns(new GenericIdentity("test"));

            GenerateJwtFlow generateJwtFlow = new(jwtService.Object, logger.Object);

            // Act
            JwtInfo actualJwtInfo = await generateJwtFlow.ExecuteAsync(generateJwtInfo, principal.Object);

            // Assert
            actualJwtInfo.Should().BeEquivalentTo(expectedJwtInfo, $"because {nameof(GenerateJwtFlow)} works");
        }
    }
}
