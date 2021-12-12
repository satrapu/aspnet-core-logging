namespace Todo.Services.Security
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;

    using FluentAssertions;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Contains unit tests targeting <see cref="PrincipalExtensions"/> class.
    /// </summary>
    [TestFixture]
    public class PrincipalExtensionsTests
    {
        /// <summary>
        /// Tests <see cref="PrincipalExtensions.GetName"/> method.
        /// </summary>
        [Test]
        public void GetName_UsingValidPrincipal_MustReturnExpectedName()
        {
            // Arrange
            const string authenticationType = "hard-coded-authentication-type-for-testing-purposes";

            IPrincipal validPrincipal = new TestClaimsPrincipal(
                     new ClaimsIdentity(
                         new GenericIdentity($"{nameof(ClaimsPrincipal)}-{Guid.NewGuid():N}", authenticationType),
                         new List<Claim>
                         {
                            new Claim(ClaimTypes.Email, $"person-{Guid.NewGuid():N}@server.net")
                         }));

            // Act
            string userName = validPrincipal.GetName();

            // Assert
            userName.Should().NotBeNullOrWhiteSpace("because principal name must mean something");
        }

        /// <summary>
        /// Tests <see cref="PrincipalExtensions.GetName"/> method.
        /// </summary>
        [Test]
        public void GetName_UsingNullAsPrincipal_MustThrowException()
        {
            // Arrange
            IPrincipal principal = null;

            // Act
            // ReSharper disable once InvokeAsExtensionMethod
            // ReSharper disable once ExpressionIsAlwaysNull
            Action getNameAction = () => PrincipalExtensions.GetName(principal);

            // Assert
            getNameAction
                .Should().ThrowExactly<ArgumentNullException>()
                .And.ParamName.Should().Be(nameof(principal), "because a null principal does not have a name");
        }

        /// <summary>
        /// Tests <see cref="PrincipalExtensions.GetName"/> method.
        /// </summary>
        [Test]
        public void GetName_UsingNullAsPrincipalIdentity_MustThrowException()
        {
            // Arrange
            var principalMock = new Mock<IPrincipal>();
            principalMock.SetupGet(principal => principal.Identity).Returns((IIdentity) null);
            IPrincipal mockedPrincipal = principalMock.Object;

            // Act
            Action getNameAction = () => mockedPrincipal.GetName();

            // Assert
            getNameAction
                .Should().ThrowExactly<ArgumentException>()
                .And.ParamName.Should().Be("principal",
                    "because a principal with a null identity does not have a name");
        }

        /// <summary>
        /// Class used for displaying a meaningful parameter description when running a parameterized test.
        /// </summary>
        private class TestClaimsPrincipal : ClaimsPrincipal
        {
            public TestClaimsPrincipal(ClaimsIdentity claimsIdentity) : base(claimsIdentity)
            {
            }

            public override string ToString()
            {
                return $"[{nameof(IIdentity.Name)}={Identity?.Name}; "
                    + $"{nameof(IIdentity.AuthenticationType)}={Identity?.AuthenticationType}]";
            }
        }
    }
}
