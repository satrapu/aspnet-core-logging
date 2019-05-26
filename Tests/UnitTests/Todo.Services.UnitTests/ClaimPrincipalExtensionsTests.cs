using FluentAssertions;
using System.Reflection;
using System.Security.Claims;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="ClaimPrincipalExtensions"/> class.
    /// </summary>
    public class ClaimPrincipalExtensionsTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public ClaimPrincipalExtensionsTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests <see cref="ClaimPrincipalExtensions.GetUserId"/> method.
        /// </summary>
        [Fact]
        public void GetUserById_UsingNullAsClaimsPrincipal_MustThrowException()
        {
            // Arrange
            testOutputHelper.WriteLine($"Running test method: {GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");
            ClaimsPrincipal nullClaimsPrincipal = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            // ReSharper disable once InvokeAsExtensionMethod
            var exception = Record.Exception(() => ClaimPrincipalExtensions.GetUserId(nullClaimsPrincipal));

            // Assert
            exception.Should().NotBeNull();
        }
    }
}
