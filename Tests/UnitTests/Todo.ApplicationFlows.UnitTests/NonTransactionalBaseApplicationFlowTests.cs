namespace Todo.ApplicationFlows
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.Extensions.Logging.Abstractions;

    using NUnit.Framework;

    /// <summary>
    /// Contains unit tests targeting <seealso cref="NonTransactionalBaseApplicationFlow{TInput, TOuput}"/> class.
    /// </summary>
    [TestFixture]
    public class NonTransactionalBaseApplicationFlowTests
    {
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1806:Do not ignore method results",
            Justification = "Newly created instance is discarded for testing purposes")]
        public void Constructor_WhenInvokedUsingNullAsFlowName_ThrowsException()
        {
            // Arrange & Act
            // ReSharper disable once ObjectCreationAsStatement
            Action constructorAction = () => new ApplicationFlowUsingNullAsFlowName();

            // Assert
            const string because = "because flow must have a name";
            constructorAction.Should().ThrowExactly<ArgumentException>(because)
                .And.ParamName.Should().Be("flowName", because);
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1806:Do not ignore method results",
            Justification = "Newly created instance is discarded for testing purposes")]
        public void Constructor_WhenInvokedUsingNullAsLogger_ThrowsException()
        {
            // Arrange & Act
            // ReSharper disable once ObjectCreationAsStatement
            Action constructorAction = () => new ApplicationFlowUsingNullAsLogger();

            // Assert
            const string because = "because flow must have a logger";
            constructorAction.Should().ThrowExactly<ArgumentNullException>(because)
                .And.ParamName.Should().Be("logger", because);
        }

        private class ApplicationFlowUsingNullAsFlowName : NonTransactionalBaseApplicationFlow<object, object>
        {
            public ApplicationFlowUsingNullAsFlowName() : base(flowName: null, NullLogger.Instance)
            {
            }

            protected override Task<object> ExecuteFlowStepsAsync(object input, IPrincipal flowInitiator)
            {
                throw new NotImplementedException();
            }
        }

        private class ApplicationFlowUsingNullAsLogger : NonTransactionalBaseApplicationFlow<object, object>
        {
            public ApplicationFlowUsingNullAsLogger() : base("TestFlow", logger: null)
            {
            }

            protected override Task<object> ExecuteFlowStepsAsync(object input, IPrincipal flowInitiator)
            {
                throw new NotImplementedException();
            }
        }
    }
}
