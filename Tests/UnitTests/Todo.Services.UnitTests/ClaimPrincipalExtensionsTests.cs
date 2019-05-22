using FluentAssertions;
using System.Security.Claims;
using Xunit;

namespace Todo.Services.UnitTests
{
    /// <summary>
    /// Contains unit tests targeting <see cref="ClaimPrincipalExtensions"/> class.
    /// </summary>
    public class ClaimPrincipalExtensionsTests
    {
        /// <summary>
        /// Tests <see cref="ClaimPrincipalExtensions.GetUserId"/> method.
        /// </summary>
        [Fact]
        public void GetUserById_UsingNullAsClaimsPrincipal_MustThrowException()
        {
            // Arrange
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
