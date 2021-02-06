using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Todo.Services.TodoItemLifecycleManagement;
using Todo.TestInfrastructure;

namespace Todo.ApplicationFlows
{
    /// <summary>
    /// Contains integration tests targeting <see cref="TransactionalBaseApplicationFlow{TInput,TOutput}"/> class.
    /// </summary>
    [TestFixture]
    public class TransactionalBaseApplicationFlowTests
    {
        private TestWebApplicationFactory testWebApplicationFactory;

        [OneTimeSetUp]
        public void GivenAnApplicationFlowIsToBeExecuted()
        {
            testWebApplicationFactory =
                new TestWebApplicationFactory(MethodBase.GetCurrentMethod()?.DeclaringType?.Name);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            testWebApplicationFactory?.Dispose();
        }

        [Test]
        public void ExecuteAsync_WhenOneFlowStepThrowsException_MustThrowException()
        {
            // Arrange
            ITodoItemService todoItemService =
                testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            ILoggerFactory loggerFactory = testWebApplicationFactory.Services.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<ApplicationFlowServingTestingPurposes>();

            // This flow is expected to fail since the service is unable to persist invalid models
            async Task<object> FlowExpectedToThrowExceptionAsync()
            {
                await todoItemService.AddAsync(new NewTodoItemInfo());
                await todoItemService.AddAsync(new NewTodoItemInfo());
                await todoItemService.AddAsync(new NewTodoItemInfo());
                return null;
            }

            string userName = $"test-user--{Guid.NewGuid():N}";
            IIdentity identity = new GenericIdentity(userName);

            string[] roles = {$"role--{Guid.NewGuid():N}"};
            IPrincipal flowInitiator = new GenericPrincipal(identity, roles);

            var applicationFlow = new ApplicationFlowServingTestingPurposes(FlowExpectedToThrowExceptionAsync, logger);

            // Act & Assert
            Assert.ThrowsAsync<ValidationException>(
                async () => await applicationFlow.ExecuteAsync(input: null, flowInitiator),
                "application flow must fail in case of an error");
        }

        private class ApplicationFlowServingTestingPurposes : TransactionalBaseApplicationFlow<object, object>
        {
            private readonly Func<Task<object>> applicationFlow;

            public ApplicationFlowServingTestingPurposes(Func<Task<object>> applicationFlow, ILogger logger)
                : base(nameof(ApplicationFlowServingTestingPurposes), logger)
            {
                this.applicationFlow = applicationFlow;
            }

            protected override async Task<object> ExecuteFlowStepsAsync(object input, IPrincipal flowInitiator)
            {
                return await applicationFlow();
            }
        }
    }
}