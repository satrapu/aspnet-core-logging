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
    /// Contains unit tests targeting <seealso cref="GenerateJwtFlow"/> class.
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
            var logger = new Mock<ILogger<GenerateJwtFlow>>();

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
            var jwtService = new Mock<IJwtService>();
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
