using FluentAssertions;
using NUnit.Framework;
using System;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains unit tests targeting <see cref="LoggingMiddlewareExtensions"/> class.
    /// </summary>
    [TestFixture]
    public class LoggingMiddlewareExtensionsTests
    {
        /// <summary>
        /// Tests <see cref="LoggingMiddlewareExtensions.UseHttpLogging"/> method.
        /// </summary>
        [Test]
        public void UseHttpLogging_UsingNullApplicationBuilder_MustThrowException()
        {
            try
            {
                LoggingMiddlewareExtensions.UseHttpLogging(null);
            }
            catch (Exception expectedException)
            {
                expectedException.Should()
                                 .NotBeNull()
                                 .And.BeAssignableTo<ArgumentNullException>();
            }
        }
    }
}
