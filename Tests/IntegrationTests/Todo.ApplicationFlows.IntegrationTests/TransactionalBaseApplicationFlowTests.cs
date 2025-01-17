namespace Todo.ApplicationFlows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using System.Transactions;

    using Commons.Constants;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using NUnit.Framework;

    using Services.TodoItemManagement;

    using VerifyNUnit;

    using VerifyTests;

    using WebApi.TestInfrastructure;

    /// <summary>
    /// Contains integration tests targeting <see cref="TransactionalBaseApplicationFlow{TInput,TOutput}"/> class.
    /// </summary>
    [SuppressMessage("ReSharper", "S1135", Justification = "The todo word represents an entity")]
    [TestFixture]
    public class TransactionalBaseApplicationFlowTests
    {
        private TestWebApplicationFactory testWebApplicationFactory;

        [OneTimeSetUp]
        public async Task GivenAnApplicationFlowIsToBeExecuted()
        {
            testWebApplicationFactory = await TestWebApplicationFactory.CreateAsync
            (
                applicationName: nameof(TransactionalBaseApplicationFlowTests),
                environmentName: EnvironmentNames.IntegrationTests,
                shouldRunStartupLogicTasks: true
            );
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

            string[] roles = [$"role--{Guid.NewGuid():N}"];
            IPrincipal flowInitiator = new GenericPrincipal(identity, roles);

            ITodoItemService todoItemService = testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            ILoggerFactory loggerFactory = testWebApplicationFactory.Services.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<ApplicationFlowServingTestingPurposes>();
            string namePrefix = $"todo-item--{Guid.NewGuid():N}";

            ITodoItemService localTodoItemService = todoItemService;

            // This flow is expected to fail since the service is unable to persist invalid models
            async Task<object> FlowExpectedToThrowExceptionAsync()
            {
                await localTodoItemService.AddAsync(new NewTodoItemInfo()
                {
                    Name = $"{namePrefix}--#1",
                    Owner = flowInitiator
                });

                await localTodoItemService.AddAsync(new NewTodoItemInfo()
                {
                    Name = $"{namePrefix}--#2",
                    Owner = flowInitiator
                });

                await localTodoItemService.AddAsync(new NewTodoItemInfo()
                {
                    Name = $"{namePrefix}--#3",
                    Owner = flowInitiator
                });

                return null;
            }

            ApplicationFlowOptions applicationFlowOptions = testWebApplicationFactory.Services.GetRequiredService<ApplicationFlowOptions>();
            ApplicationFlowServingTestingPurposes applicationFlow = new(FlowExpectedToThrowExceptionAsync, applicationFlowOptions, logger);

            // Act
            Func<Task> executeAsyncCall = async () => await applicationFlow.ExecuteAsync(input: null, flowInitiator);

            // Assert
            using AssertionScope _ = new();
            await executeAsyncCall.Should().ThrowExactlyAsync<ValidationException>("application flow must fail in case of an error");

            TodoItemQuery query = new()
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
            VerifySettings verifySettings = new(ModuleInitializer.VerifySettings);
            verifySettings.ScrubMember("Id");

            string userName = $"test-user--{Guid.NewGuid():D}";
            IIdentity identity = new GenericIdentity(userName);

            string[] roles = [$"role--{Guid.NewGuid():D}"];
            IPrincipal flowInitiator = new GenericPrincipal(identity, roles);

            ITodoItemService todoItemService = testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            ILoggerFactory loggerFactory = testWebApplicationFactory.Services.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<ApplicationFlowServingTestingPurposes>();
            string namePrefix = $"todo-item--{Guid.NewGuid():D}";

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

            ApplicationFlowOptions applicationFlowOptions = testWebApplicationFactory.Services.GetRequiredService<ApplicationFlowOptions>();
            ApplicationFlowServingTestingPurposes applicationFlow = new(FlowExpectedToSucceedAsync, applicationFlowOptions, logger);

            // Act
            await applicationFlow.ExecuteAsync(input: null, flowInitiator);

            // Assert
            TodoItemQuery query = new()
            {
                Owner = flowInitiator,
                NamePattern = $"{namePrefix}%"
            };

            // Get a new instance of ITodoItemService service to ensure data will be fetched from a new DbContext.
            todoItemService = testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            IList<TodoItemInfo> actualList = await todoItemService.GetByQueryAsync(query);

            await Verifier.Verify(actualList, settings: verifySettings);
        }

        /// <summary>
        /// Ensures <see cref="NonTransactionalBaseApplicationFlow{TInput,TOutput}.ExecuteAsync"/> method fails in case
        /// of a transaction timeout.
        /// </summary>
        /// <returns></returns>
        [Test]
        [Ignore(reason: "Test has some chance of failing when being executed by Azure Pipelines")]
        public async Task ExecuteAsync_WhenTransactionTimesOut_MustThrowException()
        {
            // Arrange
            TimeSpan transactionTimeOut = TimeSpan.FromMilliseconds(1);
            TimeSpan biggerTimeout = TimeSpan.FromMilliseconds(1000);

            string userName = $"test-user--{Guid.NewGuid():N}";
            IIdentity identity = new GenericIdentity(userName);

            string[] roles = [$"role--{Guid.NewGuid():N}"];
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
                await localTodoItemService.AddAsync(new NewTodoItemInfo()
                {
                    Name = $"{namePrefix}--#1",
                    IsComplete = false,
                    Owner = flowInitiator
                });

                await localTodoItemService.AddAsync(new NewTodoItemInfo()
                {
                    Name = $"{namePrefix}--#2",
                    IsComplete = false,
                    Owner = flowInitiator
                });

                // Ensure this flow step will take more time to execute than the configured transaction timeout used
                // by the application flow.
                await Task.Delay(biggerTimeout);

                return null;
            }

            TodoItemQuery query = new()
            {
                Owner = flowInitiator,
                NamePattern = $"{namePrefix}%"
            };

            ApplicationFlowOptions applicationFlowOptions = testWebApplicationFactory.Services.GetRequiredService<ApplicationFlowOptions>();
            // Ensure the application flow will use a very short timeout value for its transaction.
            applicationFlowOptions.TransactionOptions.Timeout = transactionTimeOut;

            ApplicationFlowServingTestingPurposes applicationFlow = new(FlowExpectedToFailAsync, applicationFlowOptions, logger);

            // Act
            Func<Task> executeAsyncCall = async () => await applicationFlow.ExecuteAsync(input: null, flowInitiator);

            // Get a new instance of ITodoItemService service to ensure data will be fetched from
            // a new DbContext.
            todoItemService = testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            IList<TodoItemInfo> list = await todoItemService.GetByQueryAsync(query);

            // Assert
            using AssertionScope _ = new();
            await executeAsyncCall.Should().ThrowExactlyAsync<TransactionAbortedException>("application flow must fail in case of transaction timeout");
            list.Count.Should().Be(expected: 0, "no entities must be created in the event of a transaction timeout");
        }

        /// <summary>
        /// A <see cref="TransactionalBaseApplicationFlow{TInput,TOutput}"/> which executes a given delegate.
        /// <br/>
        /// This class is intended to be used only for testing purposes.
        /// </summary>
        private class ApplicationFlowServingTestingPurposes : TransactionalBaseApplicationFlow<object, object>
        {
            private readonly Func<Task<object>> applicationFlow;

            public ApplicationFlowServingTestingPurposes(Func<Task<object>> applicationFlow, ApplicationFlowOptions applicationFlowOptions, ILogger logger)
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
