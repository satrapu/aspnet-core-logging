using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Todo.WebApi.Infrastructure;
using Todo.WebApi.Models;

namespace Todo.WebApi.Controllers
{
    /// <summary>
    ///  Contains integration tests targeting <see cref="TodoController" /> class.
    ///  <br/>
    ///  Based on: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.1#aspnet-core-integration-tests.
    ///  and: https://medium.com/@daniel.edwards_82928/using-webapplicationfactory-with-nunit-817a616e26f9.
    /// </summary>
    [TestFixture]
    public class TodoControllerTests
    {
        private TestWebApplicationFactory testWebApplicationFactory;

        [OneTimeSetUp]
        public void GivenAnHttpRequestIsToBePerformed()
        {
            testWebApplicationFactory = new TestWebApplicationFactory();
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.Create" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Create_UsingValidTodoItem_ReturnsTheSameInstance()
        {
            // Arrange
            using (var client = testWebApplicationFactory.CreateClient())
            {
                long? id = null;

                try
                {
                    var newTodoItemModel = new NewTodoItemModel
                    {
                        Name = $"it--{nameof(Create_UsingValidTodoItem_ReturnsTheSameInstance)}--{Guid.NewGuid():N}"
                      , IsComplete = DateTime.UtcNow.Ticks % 2 == 0
                    };

                    // Act
                    var response = await client.PostAsJsonAsync("api/todo", newTodoItemModel).ConfigureAwait(false);

                    // Assert
                    response.IsSuccessStatusCode.Should().BeTrue();
                    response.StatusCode.Should().Be(HttpStatusCode.Created);

                    var todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>().ConfigureAwait(false);
                    id = todoItemModel.Id;
                    todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete);
                    todoItemModel.Name.Should().Be(newTodoItemModel.Name);
                    todoItemModel.Id.Should().BeGreaterOrEqualTo(1);
                }
                finally
                {
                    if (id.HasValue)
                    {
                        var response = await client.DeleteAsync($"api/todo/{id}").ConfigureAwait(false);
                        response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                    }
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByQuery" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByQuery_UsingDefaults_ReturnsExpectedResult()
        {
            // Arrange
            using (var client = testWebApplicationFactory.CreateClient())
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

                    var response = await client.PostAsJsonAsync("api/todo", newTodoItemModel).ConfigureAwait(false);
                    response.IsSuccessStatusCode.Should().BeTrue();
                    response.StatusCode.Should().Be(HttpStatusCode.Created);
                    var todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>().ConfigureAwait(false);
                    id = todoItemModel.Id;

                    var parametersToAdd = new Dictionary<string, string>
                    {
                        {
                            "NamePattern", $"%-{nameSuffix}"
                        }
                    };

                    var requestUri = QueryHelpers.AddQueryString("api/todo", parametersToAdd);

                    // Act
                    response = await client.GetAsync(requestUri).ConfigureAwait(false);

                    // Assert
                    response.IsSuccessStatusCode.Should().BeTrue();
                    response.StatusCode.Should().Be(HttpStatusCode.OK);

                    var todoItemModels = await response.Content.ReadAsAsync<List<TodoItemModel>>().ConfigureAwait(false);
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
                        var response = await client.DeleteAsync($"api/todo/{id}").ConfigureAwait(false);
                        response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                    }
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.Update" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Update_UsingNewlyCreatedTodoItem_MustSucceed()
        {
            // Arrange
            using (var client = testWebApplicationFactory.CreateClient())
            {
                long? id = null;

                try
                {
                    var name = $"it--{nameof(Update_UsingNewlyCreatedTodoItem_MustSucceed)}--{Guid.NewGuid():N}";
                    var isComplete = DateTime.UtcNow.Ticks % 2 == 0;

                    var newTodoItemInfo = new NewTodoItemModel
                    {
                        Name = name, IsComplete = isComplete
                    };

                    var response = await client.PostAsJsonAsync("api/todo", newTodoItemInfo).ConfigureAwait(false);
                    response.IsSuccessStatusCode.Should().BeTrue("a new entity has been created");

                    var todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>().ConfigureAwait(false);
                    id = todoItemModel.Id;

                    var updateTodoItemModel = new UpdateTodoItemModel
                    {
                        IsComplete = !newTodoItemInfo.IsComplete, Name = $"CHANGED--{newTodoItemInfo.Name}"
                    };

                    // Act
                    response = await client.PutAsJsonAsync($"api/todo/{id}", updateTodoItemModel).ConfigureAwait(false);

                    // Assert
                    response.IsSuccessStatusCode.Should().BeTrue("an entity has been previously created");
                    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                    var parametersToAdd = new Dictionary<string, string>
                    {
                        {
                            "id", id.Value.ToString()
                        }
                    };

                    var requestUri = QueryHelpers.AddQueryString("api/todo", parametersToAdd);

                    response = await client.GetAsync(requestUri).ConfigureAwait(false);
                    response.IsSuccessStatusCode.Should().BeTrue("an entity has been previously update");
                    response.StatusCode.Should().Be(HttpStatusCode.OK);

                    var todoItemModels = await response.Content.ReadAsAsync<List<TodoItemModel>>().ConfigureAwait(false);
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
                        var response = await client.DeleteAsync($"api/todo/{id}").ConfigureAwait(false);
                        response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                    }
                }
            }
        }
    }
}