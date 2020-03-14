using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;
using Todo.Persistence.Entities;
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
            testWebApplicationFactory =
                new TestWebApplicationFactory(MethodBase.GetCurrentMethod().DeclaringType?.Name);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            testWebApplicationFactory?.Dispose();
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.Create" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Create_UsingValidTodoItem_ReturnsTheSameInstance()
        {
            // Arrange
            using HttpClient httpClient =
                await testWebApplicationFactory.CreateClientWithJwtToken().ConfigureAwait(false);
            long? id = null;

            try
            {
                var newTodoItemModel = new NewTodoItemModel
                {
                    Name = $"it--{nameof(Create_UsingValidTodoItem_ReturnsTheSameInstance)}--{Guid.NewGuid():N}",
                    IsComplete = DateTime.UtcNow.Ticks % 2 == 0
                };

                // Act
                var response = await httpClient.PostAsJsonAsync("api/todo", newTodoItemModel).ConfigureAwait(false);

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.Created);

                id = await response.Content.ReadAsAsync<long>().ConfigureAwait(false);
                id.Should().BeGreaterOrEqualTo(1);
            }
            finally
            {
                if (id.HasValue)
                {
                    var response = await httpClient.DeleteAsync($"api/todo/{id}").ConfigureAwait(false);
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByQuery" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByQueryAsync_UsingDefaults_ReturnsExpectedResult()
        {
            // Arrange
            using HttpClient httpClient =
                await testWebApplicationFactory.CreateClientWithJwtToken().ConfigureAwait(false);
            long? id = null;

            try
            {
                var nameSuffix = Guid.NewGuid().ToString("N");
                var name = $"it--{nameof(GetByQueryAsync_UsingDefaults_ReturnsExpectedResult)}--{nameSuffix}";

                var newTodoItemModel = new NewTodoItemModel
                {
                    IsComplete = DateTime.UtcNow.Ticks % 2 == 0,
                    Name = name
                };

                var response = await httpClient.PostAsJsonAsync("api/todo", newTodoItemModel).ConfigureAwait(false);
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                id = await response.Content.ReadAsAsync<long>().ConfigureAwait(false);

                var queryString = new Dictionary<string, string>
                {
                    {nameof(TodoItemQueryModel.Id), id.ToString()},
                    {nameof(TodoItemQueryModel.IsComplete), newTodoItemModel.IsComplete.ToString()},
                    {nameof(TodoItemQueryModel.NamePattern), name},
                    {nameof(TodoItemQueryModel.PageIndex), 0.ToString()},
                    {nameof(TodoItemQueryModel.PageSize), 5.ToString()},
                    {nameof(TodoItemQueryModel.SortBy), nameof(TodoItem.CreatedOn)},
                    {nameof(TodoItemQueryModel.IsSortAscending), bool.FalseString}
                };

                var requestUri = QueryHelpers.AddQueryString("api/todo", queryString);

                // Act
                response = await httpClient.GetAsync(requestUri).ConfigureAwait(false);

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var todoItemModels =
                    await response.Content.ReadAsAsync<List<TodoItemModel>>().ConfigureAwait(false);
                todoItemModels.Should().HaveCount(1);

                TodoItemModel todoItemModel = todoItemModels.Single();
                todoItemModel.Should().NotBeNull();
                todoItemModel.Name.Should().Be(newTodoItemModel.Name);
                todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete);
            }
            finally
            {
                if (id.HasValue)
                {
                    var response = await httpClient.DeleteAsync($"api/todo/{id}").ConfigureAwait(false);
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetById" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetById_UsingNewlyCreatedItem_ReturnsExpectedResult()
        {
            // Arrange
            using HttpClient httpClient =
                await testWebApplicationFactory.CreateClientWithJwtToken().ConfigureAwait(false);
            long? id = null;

            try
            {
                var nameSuffix = Guid.NewGuid().ToString("N");
                var name = $"it--{nameof(GetById_UsingNewlyCreatedItem_ReturnsExpectedResult)}--{nameSuffix}";

                var newTodoItemModel = new NewTodoItemModel
                {
                    IsComplete = DateTime.UtcNow.Ticks % 2 == 0,
                    Name = name
                };

                var response = await httpClient.PostAsJsonAsync("api/todo", newTodoItemModel).ConfigureAwait(false);
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                id = await response.Content.ReadAsAsync<long>().ConfigureAwait(false);

                // Act
                response = await httpClient.GetAsync($"api/todo/{id}").ConfigureAwait(false);

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                TodoItemModel todoItemModel =
                    await response.Content.ReadAsAsync<TodoItemModel>().ConfigureAwait(false);
                todoItemModel.Should().NotBeNull();
                todoItemModel.Id.Should().Be(id);
                todoItemModel.Name.Should().Be(newTodoItemModel.Name);
                todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete);
            }
            finally
            {
                if (id.HasValue)
                {
                    var response = await httpClient.DeleteAsync($"api/todo/{id}").ConfigureAwait(false);
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetById" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetById_UsingNonExistingId_ReturnsNotFoundStatusCode()
        {
            // Arrange
            using HttpClient httpClient =
                await testWebApplicationFactory.CreateClientWithJwtToken().ConfigureAwait(false);
            long? id = null;

            try
            {
                var nameSuffix = Guid.NewGuid().ToString("N");
                var name = $"it--{nameof(GetById_UsingNewlyCreatedItem_ReturnsExpectedResult)}--{nameSuffix}";

                var newTodoItemModel = new NewTodoItemModel
                {
                    IsComplete = DateTime.UtcNow.Ticks % 2 == 0,
                    Name = name
                };

                var response = await httpClient.PostAsJsonAsync("api/todo", newTodoItemModel).ConfigureAwait(false);
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                id = await response.Content.ReadAsAsync<long>().ConfigureAwait(false);
                long nonExistentId = long.MinValue;

                // Act
                response = await httpClient.GetAsync($"api/todo/{nonExistentId}").ConfigureAwait(false);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                    "must not find something which does not exist");
            }
            finally
            {
                if (id.HasValue)
                {
                    var response = await httpClient.DeleteAsync($"api/todo/{id}").ConfigureAwait(false);
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
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
            using HttpClient httpClient =
                await testWebApplicationFactory.CreateClientWithJwtToken().ConfigureAwait(false);
            long? id = null;

            try
            {
                var name = $"it--{nameof(Update_UsingNewlyCreatedTodoItem_MustSucceed)}--{Guid.NewGuid():N}";
                var isComplete = DateTime.UtcNow.Ticks % 2 == 0;

                var newTodoItemInfo = new NewTodoItemModel
                {
                    Name = name, IsComplete = isComplete
                };

                var response = await httpClient.PostAsJsonAsync("api/todo", newTodoItemInfo).ConfigureAwait(false);
                response.IsSuccessStatusCode.Should().BeTrue("a new entity has been created");

                id = await response.Content.ReadAsAsync<long>().ConfigureAwait(false);

                var updateTodoItemModel = new UpdateTodoItemModel
                {
                    IsComplete = !newTodoItemInfo.IsComplete, Name = $"CHANGED--{newTodoItemInfo.Name}"
                };

                // Act
                response = await httpClient.PutAsJsonAsync($"api/todo/{id}", updateTodoItemModel)
                    .ConfigureAwait(false);

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

                response = await httpClient.GetAsync(requestUri).ConfigureAwait(false);
                response.IsSuccessStatusCode.Should().BeTrue("an entity has been previously update");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var todoItemModels =
                    await response.Content.ReadAsAsync<List<TodoItemModel>>().ConfigureAwait(false);
                todoItemModels.Should().HaveCount(1, "the query is using id only");

                TodoItemModel todoItemModel = todoItemModels.Single();
                todoItemModel.Id.Should().Be(id);
                todoItemModel.IsComplete.Should().Be(updateTodoItemModel.IsComplete);
                todoItemModel.Name.Should().Be(updateTodoItemModel.Name);
            }
            finally
            {
                if (id.HasValue)
                {
                    var response = await httpClient.DeleteAsync($"api/todo/{id}").ConfigureAwait(false);
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                }
            }
        }
    }
}