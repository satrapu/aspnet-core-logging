using FluentAssertions;
using NUnit.Framework;
using System;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="ClaimPrincipalExtensions"/> class.
    /// </summary>
    [TestFixture]
    public class ClaimPrincipalExtensionsTests
    {
        /// <summary>
        /// Tests <see cref="ClaimPrincipalExtensions.GetUserId"/> method.
        /// </summary>
        [Test]
        public void GetUserById_UsingNullAsClaimsPrincipal_MustThrowException()
        {
            try
            {
                ClaimPrincipalExtensions.GetUserId(null);
            }
            catch (Exception expectedException)
            {
                expectedException.Should()
                                 .BeAssignableTo<ArgumentNullException>()
                                 .And.Subject.As<ArgumentNullException>()
                                 .ParamName.Should()
                                 .Be("claimsPrincipal");
            }
        }
    }
}
