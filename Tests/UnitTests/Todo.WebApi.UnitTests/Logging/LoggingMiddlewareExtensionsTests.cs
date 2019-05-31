using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using System;
using Xunit;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains unit tests targeting <see cref="LoggingMiddlewareExtensions"/> class.
    /// </summary>
    public class LoggingMiddlewareExtensionsTests
    {
        /// <summary>
        /// Tests <see cref="LoggingMiddlewareExtensions.UseHttpLogging"/> method.
        /// </summary>
        [Fact]
        public void UseHttpLogging_UsingNullApplicationBuilder_MustThrowException()
        {
            IApplicationBuilder applicationBuilder = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            var exception = Record.Exception(() => applicationBuilder.UseHttpLogging());

            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>();
        }
    }
}
