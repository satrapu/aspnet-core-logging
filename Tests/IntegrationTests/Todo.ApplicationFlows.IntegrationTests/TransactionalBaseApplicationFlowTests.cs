namespace Todo.ApplicationFlows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using System.Transactions;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using NUnit.Framework;

    using Services.TodoItemLifecycleManagement;

    using TestInfrastructure;

    /// <summary>
    /// Contains integration tests targeting <see cref="TransactionalBaseApplicationFlow{TInput,TOutput}"/> class.
    /// </summary>
    [SuppressMessage("ReSharper", "S1135", Justification = "The todo word represents an entity")]
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

        /// <summary>
        /// Ensures method <see cref="TransactionalBaseApplicationFlow{TInput,TOutput}.ExecuteAsync" />
        /// works as expected.
        /// <br/>
        /// Given an application flow containing several steps which are expected to fail, then the entire flow must
        /// fail too. Each such step is supposed to persist one todo item to the underlying database, but since the
        /// flow will fail, the test checks whether database contain any such items.
        /// </summary>
        [Test]
        public async Task ExecuteAsync_WhenOneFlowStepThrowsException_MustThrowException()
        {
            // Arrange
            string userName = $"test-user--{Guid.NewGuid():N}";
            IIdentity identity = new GenericIdentity(userName);

            string[] roles = { $"role--{Guid.NewGuid():N}" };
            IPrincipal flowInitiator = new GenericPrincipal(identity, roles);

            ITodoItemService todoItemService =
                testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            ILoggerFactory loggerFactory = testWebApplicationFactory.Services.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<ApplicationFlowServingTestingPurposes>();
            string namePrefix = $"todo-item--{Guid.NewGuid():N}";

            ITodoItemService localTodoItemService = todoItemService;

            // This flow is expected to fail since the service is unable to persist invalid models
            async Task<object> FlowExpectedToThrowExceptionAsync()
            {
                await localTodoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#1",
                    Owner = flowInitiator
                });
                await localTodoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#2",
                    Owner = flowInitiator
                });
                await localTodoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#3",
                    Owner = flowInitiator
                });
                return null;
            }

            IOptionsMonitor<ApplicationFlowOptions> applicationFlowOptions =
                testWebApplicationFactory.Services.GetRequiredService<IOptionsMonitor<ApplicationFlowOptions>>();

            var applicationFlow = new ApplicationFlowServingTestingPurposes(FlowExpectedToThrowExceptionAsync,
                applicationFlowOptions, logger);

            // Act
            Func<Task> executeAsyncCall = async () => await applicationFlow.ExecuteAsync(input: null, flowInitiator);

            // Assert
            using (new AssertionScope())
            {
                executeAsyncCall.Should()
                    .ThrowExactly<ValidationException>("application flow must fail in case of an error");

                var query = new TodoItemQuery
                {
                    Owner = flowInitiator,
                    NamePattern = $"{namePrefix}%"
                };

                // Get a new instance of ITodoItemService service to ensure data will be fetched from
                // a new DbContext.
                todoItemService = testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
                IList<TodoItemInfo> list = await todoItemService.GetByQueryAsync(query);
                list.Count.Should().Be(expected: 0, "the application flow failed to persist todo items");
            }
        }

        /// <summary>
        /// Ensures method <see cref="TransactionalBaseApplicationFlow{TInput,TOutput}.ExecuteAsync" />
        /// works as expected.
        /// <br/>
        /// Several todo items will be persisted via an application flow, then this test will check whether these items
        /// can be fetched from the underlying database.
        /// </summary>
        [Test]
        public async Task ExecuteAsync_WhenAllStepsSucceeds_MustSucceed()
        {
            // Arrange
            string userName = $"test-user--{Guid.NewGuid():N}";
            IIdentity identity = new GenericIdentity(userName);

            string[] roles = { $"role--{Guid.NewGuid():N}" };
            IPrincipal flowInitiator = new GenericPrincipal(identity, roles);

            ITodoItemService todoItemService =
                testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            ILoggerFactory loggerFactory = testWebApplicationFactory.Services.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<ApplicationFlowServingTestingPurposes>();
            string namePrefix = $"todo-item--{Guid.NewGuid():N}";

            // This flow is expected to succeed since the service is persisting valid models
            ITodoItemService localTodoItemService = todoItemService;

            async Task<object> FlowExpectedToSucceedAsync()
            {
                await localTodoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#1",
                    IsComplete = false,
                    Owner = flowInitiator
                });
                await localTodoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#2",
                    IsComplete = false,
                    Owner = flowInitiator
                });
                await localTodoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#3",
                    IsComplete = false,
                    Owner = flowInitiator
                });
                return null;
            }

            IOptionsMonitor<ApplicationFlowOptions> applicationFlowOptions =
                testWebApplicationFactory.Services.GetRequiredService<IOptionsMonitor<ApplicationFlowOptions>>();

            var applicationFlow = new ApplicationFlowServingTestingPurposes(FlowExpectedToSucceedAsync,
                applicationFlowOptions, logger);

            // Act
            await applicationFlow.ExecuteAsync(input: null, flowInitiator);

            // Assert
            var query = new TodoItemQuery
            {
                Owner = flowInitiator,
                NamePattern = $"{namePrefix}%"
            };

            // Get a new instance of ITodoItemService service to ensure data will be fetched from
            // a new DbContext.
            todoItemService = testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            IList<TodoItemInfo> list = await todoItemService.GetByQueryAsync(query);
            list.Count.Should().Be(expected: 3, "several todo items have been previously created");
        }

        /// <summary>
        /// Ensures <see cref="NonTransactionalBaseApplicationFlow{TInput,TOutput}.ExecuteAsync"/> method fails in case
        /// of a transaction timeout.
        /// </summary>
        /// <returns></returns>
        [Test]
        [Ignore("I need to figure a better way of triggering throwing a TransactionAbortedException since the current "
                + "approach fails sometimes")]
        public async Task ExecuteAsync_WhenTransactionTimesOut_MustThrowException()
        {
            // Arrange
            TimeSpan transactionTimeOut = TimeSpan.FromMilliseconds(1);
            TimeSpan biggerTimeout = TimeSpan.FromMilliseconds(1000);

            string userName = $"test-user--{Guid.NewGuid():N}";
            IIdentity identity = new GenericIdentity(userName);

            string[] roles = { $"role--{Guid.NewGuid():N}" };
            IPrincipal flowInitiator = new GenericPrincipal(identity, roles);

            ITodoItemService todoItemService =
                testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            ILoggerFactory loggerFactory = testWebApplicationFactory.Services.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<ApplicationFlowServingTestingPurposes>();
            string namePrefix = $"todo-item--{Guid.NewGuid():N}";

            // This flow is expected to fail
            ITodoItemService localTodoItemService = todoItemService;

            async Task<object> FlowExpectedToFailAsync()
            {
                await localTodoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#1",
                    IsComplete = false,
                    Owner = flowInitiator
                });

                await localTodoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#2",
                    IsComplete = false,
                    Owner = flowInitiator
                });

                // Ensure this flow step will take more time to execute than the configured transaction timeout used
                // by the application flow.
                Task.Delay(biggerTimeout).Wait();

                return null;
            }

            var query = new TodoItemQuery
            {
                Owner = flowInitiator,
                NamePattern = $"{namePrefix}%"
            };

            IOptionsMonitor<ApplicationFlowOptions> applicationFlowOptions =
                testWebApplicationFactory.Services.GetRequiredService<IOptionsMonitor<ApplicationFlowOptions>>();

            // Ensure the application flow will use a very short timeout value for its transaction.
            applicationFlowOptions.CurrentValue.TransactionOptions.Timeout = transactionTimeOut;

            var applicationFlow =
                new ApplicationFlowServingTestingPurposes(FlowExpectedToFailAsync, applicationFlowOptions, logger);

            // Act
            Func<Task> executeAsyncCall = async () => await applicationFlow.ExecuteAsync(input: null, flowInitiator);

            // Get a new instance of ITodoItemService service to ensure data will be fetched from
            // a new DbContext.
            todoItemService = testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            IList<TodoItemInfo> list = await todoItemService.GetByQueryAsync(query);

            // Assert
            using (new AssertionScope())
            {
                executeAsyncCall.Should()
                    .ThrowExactly<TransactionAbortedException>(
                        "application flow must fail in case of transaction timeout");

                list.Count.Should().Be(expected: 0,
                    "no entities must be created in the event of a transaction timeout");
            }
        }

        /// <summary>
        /// A <see cref="TransactionalBaseApplicationFlow{TInput,TOutput}"/> which executes a given delegate.
        /// <br/>
        /// This class is intended to be used only for testing purposes.
        /// </summary>
        private class ApplicationFlowServingTestingPurposes : TransactionalBaseApplicationFlow<object, object>
        {
            private readonly Func<Task<object>> applicationFlow;

            public ApplicationFlowServingTestingPurposes(Func<Task<object>> applicationFlow,
                IOptionsMonitor<ApplicationFlowOptions> applicationFlowOptions, ILogger logger)
                : base(nameof(ApplicationFlowServingTestingPurposes), applicationFlowOptions, logger)
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
