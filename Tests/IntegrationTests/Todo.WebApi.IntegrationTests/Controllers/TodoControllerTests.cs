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
        private const string BaseUrl = "api/todo";

        [OneTimeSetUp]
        public void GivenAnHttpRequestIsToBePerformed()
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
        /// Ensures method <see cref="TodoController.CreateAsync" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CreateAsync_UsingValidTodoItem_ReturnsTheSameInstance()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateClientWithJwtAsync();
            long? id = null;

            try
            {
                var newTodoItemModel = new NewTodoItemModel
                {
                    Name = $"it--{nameof(CreateAsync_UsingValidTodoItem_ReturnsTheSameInstance)}--{Guid.NewGuid():N}",
                    IsComplete = DateTime.UtcNow.Ticks % 2 == 0
                };

                // Act
                HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.Created);

                id = await response.Content.ReadAsAsync<long>();
                id.Should().BeGreaterOrEqualTo(1);
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByQueryAsync" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByQueryAsync_UsingDefaults_ReturnsExpectedResult()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateClientWithJwtAsync();
            long? id = null;

            try
            {
                string nameSuffix = Guid.NewGuid().ToString("N");
                string name = $"it--{nameof(GetByQueryAsync_UsingDefaults_ReturnsExpectedResult)}--{nameSuffix}";

                var newTodoItemModel = new NewTodoItemModel
                {
                    IsComplete = DateTime.UtcNow.Ticks % 2 == 0,
                    Name = name
                };

                HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                id = await response.Content.ReadAsAsync<long>();

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

                string requestUri = QueryHelpers.AddQueryString(BaseUrl, queryString);

                // Act
                response = await httpClient.GetAsync(requestUri);

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                List<TodoItemModel> todoItemModels = await response.Content.ReadAsAsync<List<TodoItemModel>>();
                todoItemModels.Should().HaveCount(1);

                TodoItemModel todoItemModel = todoItemModels.Single();
                todoItemModel.Should().NotBeNull();
                todoItemModel.Name.Should().Be(newTodoItemModel.Name);
                todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete.Value);
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByIdAsync" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByIdAsync_UsingNewlyCreatedItem_ReturnsExpectedResult()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateClientWithJwtAsync();
            long? id = null;

            try
            {
                string nameSuffix = Guid.NewGuid().ToString("N");
                string name = $"it--{nameof(GetByIdAsync_UsingNewlyCreatedItem_ReturnsExpectedResult)}--{nameSuffix}";

                var newTodoItemModel = new NewTodoItemModel
                {
                    IsComplete = DateTime.UtcNow.Ticks % 2 == 0,
                    Name = name
                };

                HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                id = await response.Content.ReadAsAsync<long>();

                // Act
                response = await httpClient.GetAsync($"{BaseUrl}/{id}");

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                TodoItemModel todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>();
                todoItemModel.Should().NotBeNull();
                todoItemModel.Id.Should().Be(id);
                todoItemModel.Name.Should().Be(newTodoItemModel.Name);
                todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete.Value);
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByIdAsync" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByIdAsync_UsingNonExistingId_ReturnsNotFoundStatusCode()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateClientWithJwtAsync();
            long? id = null;

            try
            {
                string nameSuffix = Guid.NewGuid().ToString("N");
                string name = $"it--{nameof(GetByIdAsync_UsingNonExistingId_ReturnsNotFoundStatusCode)}--{nameSuffix}";

                var newTodoItemModel = new NewTodoItemModel
                {
                    IsComplete = DateTime.UtcNow.Ticks % 2 == 0,
                    Name = name
                };

                HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                id = await response.Content.ReadAsAsync<long>();
                long nonExistentId = long.MinValue;

                // Act
                response = await httpClient.GetAsync($"{BaseUrl}/{nonExistentId}");

                // Assert
                response.StatusCode.Should()
                    .Be(HttpStatusCode.NotFound, "must not find something which does not exist");
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.UpdateAsync" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task UpdateAsync_UsingNewlyCreatedTodoItem_MustSucceed()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateClientWithJwtAsync();
            long? id = null;

            try
            {
                string name = $"it--{nameof(UpdateAsync_UsingNewlyCreatedTodoItem_MustSucceed)}--{Guid.NewGuid():N}";
                bool isComplete = DateTime.UtcNow.Ticks % 2 == 0;

                var newTodoItemInfo = new NewTodoItemModel
                {
                    Name = name,
                    IsComplete = isComplete
                };

                HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemInfo);
                response.IsSuccessStatusCode.Should().BeTrue("a new entity has been created");
                id = await response.Content.ReadAsAsync<long>();

                var updateTodoItemModel = new UpdateTodoItemModel
                {
                    IsComplete = !newTodoItemInfo.IsComplete,
                    Name = $"CHANGED--{newTodoItemInfo.Name}"
                };

                // Act
                response = await httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateTodoItemModel);

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue("an entity has been previously created");
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                response = await httpClient.GetAsync($"{BaseUrl}/{id}");
                response.IsSuccessStatusCode.Should().BeTrue("an entity has been previously updated");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                TodoItemModel todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>();
                todoItemModel.Should().NotBeNull("an entity has been previously created");
                todoItemModel.Id.Should().Be(id);
                todoItemModel.IsComplete.Should().Be(updateTodoItemModel.IsComplete.Value);
                todoItemModel.Name.Should().Be(updateTodoItemModel.Name);
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue("cleanup must succeed");
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.DeleteAsync" /> works as expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DeleteAsync_UsingNewlyCreatedTodoItem_MustSucceed()
        {
            // Arrange
            string name = $"it--{nameof(DeleteAsync_UsingNewlyCreatedTodoItem_MustSucceed)}--{Guid.NewGuid():N}";
            bool isComplete = DateTime.UtcNow.Ticks % 2 == 0;

            var newTodoItemInfo = new NewTodoItemModel
            {
                Name = name,
                IsComplete = isComplete
            };

            using HttpClient httpClient = await testWebApplicationFactory.CreateClientWithJwtAsync();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemInfo);
            response.IsSuccessStatusCode.Should().BeTrue("a new entity has been created");
            long? id = await response.Content.ReadAsAsync<long>();

            // Act
            response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue("existing entity must be deleted");
            response = await httpClient.GetAsync($"{BaseUrl}/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound, "existing entity has already been deleted");
        }
    }
}