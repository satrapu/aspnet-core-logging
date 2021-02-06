﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using FluentAssertions;
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

        /// <summary>
        /// Ensures method <see cref="TransactionalBaseApplicationFlow{TInput,TOutput}.ExecuteAsync" />
        /// works as expected.
        /// <br/>
        /// Given an application flow containing several steps which are expected to fail, then the flow must also fail.
        /// Each such step is supposed to persist one todo item to the underlying database, but since the flow will fail,
        /// the test checks whether database contain any such items.
        /// </summary>
        [Test]
        public async Task ExecuteAsync_WhenOneFlowStepThrowsException_MustThrowException()
        {
            // Arrange
            string userName = $"test-user--{Guid.NewGuid():N}";
            IIdentity identity = new GenericIdentity(userName);

            string[] roles = {$"role--{Guid.NewGuid():N}"};
            IPrincipal flowInitiator = new GenericPrincipal(identity, roles);

            ITodoItemService todoItemService =
                testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            ILoggerFactory loggerFactory = testWebApplicationFactory.Services.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<ApplicationFlowServingTestingPurposes>();
            string namePrefix = $"todo-item--{Guid.NewGuid():N}";

            // This flow is expected to fail since the service is unable to persist invalid models
            async Task<object> FlowExpectedToThrowExceptionAsync()
            {
                await todoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#1",
                    Owner = flowInitiator
                });
                await todoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#2",
                    Owner = flowInitiator
                });
                await todoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#3",
                    Owner = flowInitiator
                });
                return null;
            }

            var applicationFlow = new ApplicationFlowServingTestingPurposes(FlowExpectedToThrowExceptionAsync, logger);

            // Act & Assert
            Assert.ThrowsAsync<ValidationException>(
                async () => await applicationFlow.ExecuteAsync(input: null, flowInitiator),
                "application flow must fail in case of an error");

            var query = new TodoItemQuery
            {
                Owner = flowInitiator,
                NamePattern = $"{namePrefix}%"
            };

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
            string userName = $"test-user--{Guid.NewGuid():N}";
            IIdentity identity = new GenericIdentity(userName);

            string[] roles = {$"role--{Guid.NewGuid():N}"};
            IPrincipal flowInitiator = new GenericPrincipal(identity, roles);

            ITodoItemService todoItemService =
                testWebApplicationFactory.Services.GetRequiredService<ITodoItemService>();
            ILoggerFactory loggerFactory = testWebApplicationFactory.Services.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<ApplicationFlowServingTestingPurposes>();
            string namePrefix = $"todo-item--{Guid.NewGuid():N}";

            // This flow is expected to fail since the service is unable to persist invalid models
            async Task<object> FlowExpectedToSucceedAsync()
            {
                await todoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#1",
                    IsComplete = false,
                    Owner = flowInitiator
                });
                await todoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#2",
                    IsComplete = false,
                    Owner = flowInitiator
                });
                await todoItemService.AddAsync(new NewTodoItemInfo
                {
                    Name = $"{namePrefix}--#3",
                    IsComplete = false,
                    Owner = flowInitiator
                });
                return null;
            }

            var applicationFlow = new ApplicationFlowServingTestingPurposes(FlowExpectedToSucceedAsync, logger);

            // Act
            await applicationFlow.ExecuteAsync(input: null, flowInitiator);

            // Assert
            var query = new TodoItemQuery
            {
                Owner = flowInitiator,
                NamePattern = $"{namePrefix}%"
            };

            IList<TodoItemInfo> list = await todoItemService.GetByQueryAsync(query);
            list.Count.Should().Be(expected: 3, "several todo items have been previously created");
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