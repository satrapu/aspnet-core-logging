using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Todo.WebApi.Controllers;
using Todo.WebApi.IntegrationTests.Infrastructure;
using Todo.WebApi.Models;
using Xunit;
using Xunit.Abstractions;

namespace Todo.WebApi.IntegrationTests.Controllers
{
    /// <summary>
    ///  Contains integration tests targeting <see cref="TodoController" /> class.
    ///  <br/>
    ///  Based on: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.1#aspnet-core-integration-tests.
    /// </summary>
    public class TodoControllerTests : IClassFixture<TodoWebApplicationFactory>
    {
        private readonly TodoWebApplicationFactory testFactory;
        private readonly ITestOutputHelper testOutputHelper;

        public TodoControllerTests(TodoWebApplicationFactory testFactory, ITestOutputHelper testOutputHelper)
        {
            this.testFactory = testFactory;
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.Create" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Create_UsingValidTodoItem_ReturnsTheSameInstance()
        {
            // Arrange
            using (var client = testFactory.CreateClient())
            {
                long? id = null;

                try
                {
                    var newTodoItemModel = new NewTodoItemModel()
                    {
                        Name = $"it--{nameof(Create_UsingValidTodoItem_ReturnsTheSameInstance)}--{Guid.NewGuid():N}"
                      , IsComplete = DateTime.UtcNow.Ticks % 2 == 0
                    };

                    // Act
                    var response = await client.PostAsJsonAsync("api/todo", newTodoItemModel);
                    await response.PrintError(testOutputHelper);

                    // Assert
                    response.IsSuccessStatusCode.Should().BeTrue();
                    response.StatusCode.Should().Be(HttpStatusCode.Created);

                    var todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>();
                    id = todoItemModel.Id;
                    todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete);
                    todoItemModel.Name.Should().Be(newTodoItemModel.Name);
                    todoItemModel.Id.Should().BeGreaterOrEqualTo(1);
                }
                finally
                {
                    if (id.HasValue)
                    {
                        var response = await client.DeleteAsync($"api/todo/{id}");
                        response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                    }
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByQuery" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetByQuery_UsingDefaults_ReturnsExpectedResult()
        {
            // Arrange
            using (var client = testFactory.CreateClient())
            {
                long? id = null;

                try
                {
                    var nameSuffix = Guid.NewGuid().ToString("N");
                    var name = $"it--{nameof(GetByQuery_UsingDefaults_ReturnsExpectedResult)}--{nameSuffix}";

                    var newTodoItemModel = new NewTodoItemModel
                    {
                        IsComplete = DateTime.UtcNow.Ticks % 2 == 0, Name = name
                    };

                    var response = await client.PostAsJsonAsync("api/todo", newTodoItemModel);
                    await response.PrintError(testOutputHelper);
                    response.IsSuccessStatusCode.Should().BeTrue();
                    response.StatusCode.Should().Be(HttpStatusCode.Created);
                    var todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>();
                    id = todoItemModel.Id;

                    var parametersToAdd = new Dictionary<string, string>
                    {
                        {
                            "NamePattern", $"%-{nameSuffix}"
                        }
                    };

                    var requestUri = QueryHelpers.AddQueryString("api/todo", parametersToAdd);

                    // Act
                    response = await client.GetAsync(requestUri);
                    await response.PrintError(testOutputHelper);

                    // Assert
                    response.IsSuccessStatusCode.Should().BeTrue();
                    response.StatusCode.Should().Be(HttpStatusCode.OK);

                    var todoItemModels = await response.Content.ReadAsAsync<List<TodoItemModel>>();
                    todoItemModels.Should().HaveCount(1);

                    todoItemModel = todoItemModels.Single();
                    todoItemModel.Should().NotBeNull();
                    todoItemModel.Name.Should().Be(newTodoItemModel.Name);
                    todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete);
                }
                finally
                {
                    if (id.HasValue)
                    {
                        var response = await client.DeleteAsync($"api/todo/{id}");
                        response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                    }
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.Update" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Update_UsingNewlyCreatedTodoItem_MustSucceed()
        {
            // Arrange
            using (var client = testFactory.CreateClient())
            {
                long? id = null;

                try
                {
                    var name = $"it--{nameof(Update_UsingNewlyCreatedTodoItem_MustSucceed)}--{Guid.NewGuid():N}";
                    var isComplete = DateTime.UtcNow.Ticks % 2 == 0;

                    var newTodoItemInfo = new NewTodoItemModel
                    {
                        Name = name
                      , IsComplete = isComplete
                    };

                    var response = await client.PostAsJsonAsync("api/todo", newTodoItemInfo);
                    await response.PrintError(testOutputHelper);
                    response.IsSuccessStatusCode.Should().BeTrue("a new entity has been created");

                    var todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>();
                    id = todoItemModel.Id;

                    var updateTodoItemModel = new UpdateTodoItemModel()
                    {
                        IsComplete = !newTodoItemInfo.IsComplete
                      , Name = $"CHANGED--{newTodoItemInfo.Name}"
                    };

                    // Act
                    response = await client.PutAsJsonAsync($"api/todo/{id}", updateTodoItemModel);
                    await response.PrintError(testOutputHelper);

                    // Assert
                    response.IsSuccessStatusCode.Should().BeTrue("an entity has been previously created");
                    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                    var parametersToAdd = new Dictionary<string, string>
                    {
                        { "id", id.Value.ToString() }
                    };
                    var requestUri = QueryHelpers.AddQueryString("api/todo", parametersToAdd);

                    response = await client.GetAsync(requestUri);
                    await response.PrintError(testOutputHelper);
                    response.IsSuccessStatusCode.Should().BeTrue("an entity has been previously update");
                    response.StatusCode.Should().Be(HttpStatusCode.OK);

                    var todoItemModels = await response.Content.ReadAsAsync<List<TodoItemModel>>();
                    todoItemModels.Should().HaveCount(1, "the query is using id only");

                    todoItemModel = todoItemModels.Single();
                    todoItemModel.Id.Should().Be(id);
                    todoItemModel.IsComplete.Should().Be(updateTodoItemModel.IsComplete);
                    todoItemModel.Name.Should().Be(updateTodoItemModel.Name);
                }
                finally
                {
                    if (id.HasValue)
                    {
                        var response = await client.DeleteAsync($"api/todo/{id}");
                        response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                    }
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.Update" /> fails when using an id set to zero or negative value.
        /// </summary>
        /// <param name="invalidId"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(long.MinValue)]
        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task Update_UsingNonPositiveId_MustFail(long invalidId)
        {
            // Arrange
            using (var client = testFactory.CreateClient())
            {
                var name = $"it--{nameof(Update_UsingNonPositiveId_MustFail)}--{Guid.NewGuid():N}";
                var isComplete = DateTime.UtcNow.Ticks % 2 == 0;

                var updateTodoItemModel = new UpdateTodoItemModel()
                {
                    Name = name
                  , IsComplete = isComplete
                };

                // Act
                var response = await client.PutAsJsonAsync($"api/todo/{invalidId}", updateTodoItemModel);
                await response.PrintError(testOutputHelper);

                // Assert
                response.IsSuccessStatusCode.Should().BeFalse($"must not update an entity when using invalid id");
            }
        }
    }
}