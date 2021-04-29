namespace Todo.WebApi.Logging
{
    using System;

    using FluentAssertions;

    using NUnit.Framework;

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
            Action useHttpLogging = () => LoggingMiddlewareExtensions.UseHttpLogging(null);
            useHttpLogging.Should()
                .ThrowExactly<ArgumentNullException>("cannot enable logging middleware for a null application builder");
        }
    }
}
