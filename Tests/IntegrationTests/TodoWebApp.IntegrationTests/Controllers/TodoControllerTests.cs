using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TodoWebApp.Controllers;
using TodoWebApp.Models;
using Xunit;

namespace TodoWebApp.IntegrationTests.Controllers
{
    /// <summary>
    /// <para>
    /// Contains integration tests targeting <seealso cref="TodoController"/> class.
    /// </para>
    /// Based on: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.1#aspnet-core-integration-tests.
    /// </summary>
    public class TodoControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;

        public TodoControllerTests(WebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Ensures method <seealso cref="TodoController.GetAll"/> works as expected.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAll_UsingDefaults_ReturnsAtLeastOneTodoItem()
        {
            // Arrange
            using (var client = factory.CreateClient())
            {
                // Act
                var response = await client.GetAsync("api/todo");

                // Assert
                Assert.True(response.IsSuccessStatusCode);

                // Deserialize response content using either Newtonsoft.Json.JsonConvert class
                // or directly reading response content as an instance of a List<TodoItem> type.
                // Solution #1: Use Newtonsoft.Json.JsonConvert
                //var responseContent = await response.Content.ReadAsStringAsync();
                //Assert.NotEmpty(responseContent);
                //var todoItems = JsonConvert.DeserializeObject<List<TodoItem>>(responseContent);
                // Solution #2: response.Content.ReadAsAsync<List<TodoItem>>
                var todoItems = await response.Content.ReadAsAsync<List<TodoItem>>();
                Assert.True(todoItems.TrueForAll(todoItem => todoItem != null
                                                             && todoItem.Id >= 1
                                                             && !string.IsNullOrWhiteSpace(todoItem.Name)),
                    $"List must contain non-null {nameof(TodoItem)} instances");
            }
        }

        /// <summary>
        /// Ensures method <seealso cref="TodoController.Create"/> works as expected.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Create_UsingValidTodoItem_ReturnsTheSameInstance()
        {
            // Arrange
            using (var client = factory.CreateClient())
            {
                var name = $"todo-item-4-testing-{Guid.NewGuid():N}";
                var id = DateTime.UtcNow.Ticks;
                var isComplete = DateTime.UtcNow.Ticks % 2 == 0;

                var newTodoItem = new TodoItem
                {
                    Name = name,
                    Id = id,
                    IsComplete = isComplete
                };

                // Act
                var response = await client.PostAsJsonAsync("api/todo", newTodoItem);

                // Assert
                Assert.True(response.IsSuccessStatusCode, $"Expected to create new valid {nameof(TodoItem)} instance");

                var newlyCreatedTodoItem = await response.Content.ReadAsAsync<TodoItem>();
                Assert.Equal(name, newlyCreatedTodoItem.Name);
                Assert.Equal(id, newlyCreatedTodoItem.Id);
                Assert.Equal(isComplete, newlyCreatedTodoItem.IsComplete);
            }
        }

        /// <summary>
        /// Ensures method <seealso cref="TodoController.Create"/> fails when using an id set to zero or negative value.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(long.MinValue)]
        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task Create_UsingTodoItemWithIdSetToZeroOrNegativeValue_MustFail(long invalidId)
        {
            // Arrange
            using (var client = factory.CreateClient())
            {
                var name = $"todo-item-4-testing-{Guid.NewGuid():N}";
                var isComplete = DateTime.UtcNow.Ticks % 2 == 0;

                var newTodoItem = new TodoItem
                {
                    Name = name,
                    Id = invalidId,
                    IsComplete = isComplete
                };

                // Act
                var response = await client.PostAsJsonAsync("api/todo", newTodoItem);

                // Assert
                Assert.False(response.IsSuccessStatusCode,
                    $"Expected to not create new {nameof(TodoItem)} when using invalid id");
            }
        }

        /// <summary>
        /// Ensures method <seealso cref="TodoController.Update"/> works as expected.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Update_UsingNewlyCreatedTodoItem_PerformsTheCorrectChanges()
        {
            // Arrange
            using (var client = factory.CreateClient())
            {
                var name = $"todo-item-4-testing-{Guid.NewGuid():N}";
                var id = DateTime.UtcNow.Ticks;
                var isComplete = DateTime.UtcNow.Ticks % 2 == 0;

                var newTodoItem = new TodoItem
                {
                    Name = name,
                    Id = id,
                    IsComplete = isComplete
                };
                var response = await client.PostAsJsonAsync("api/todo", newTodoItem);
                Assert.True(response.IsSuccessStatusCode,
                    $"Expected to create a new valid {nameof(TodoItem)} instance");

                var updateTodoItem = new TodoItem
                {
                    Id = newTodoItem.Id,
                    IsComplete = !newTodoItem.IsComplete,
                    Name = $"CHANGED--{newTodoItem.Name}"
                };

                // Act
                response = await client.PutAsJsonAsync($"api/todo/{updateTodoItem.Id}", updateTodoItem);

                // Assert
                Assert.True(response.IsSuccessStatusCode,
                    $"Expected to update existing valid {nameof(TodoItem)} instance");

                var existingTodoItem = await response.Content.ReadAsAsync<TodoItem>();
                Assert.Equal(updateTodoItem.Name, existingTodoItem.Name);
                Assert.Equal(updateTodoItem.Id, existingTodoItem.Id);
                Assert.Equal(updateTodoItem.IsComplete, existingTodoItem.IsComplete);
            }
        }

        /// <summary>
        /// Ensures method <seealso cref="TodoController.Update"/> fails when using an id set to zero or negative value.
        /// </summary>
        /// <param name="invalidId"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(long.MinValue)]
        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task Update_UsingTodoItemWithIdSetToZeroOrNegativeValue_MustFail(long invalidId)
        {
            // Arrange
            using (var client = factory.CreateClient())
            {
                var name = $"todo-item-4-testing-{Guid.NewGuid():N}";
                var isComplete = DateTime.UtcNow.Ticks % 2 == 0;

                var newTodoItem = new TodoItem
                {
                    Name = name,
                    Id = invalidId,
                    IsComplete = isComplete
                };

                // Act
                var response = await client.PutAsJsonAsync("api/todo", newTodoItem);

                // Assert
                Assert.False(response.IsSuccessStatusCode,
                    $"Expected to not update {nameof(TodoItem)} instance when using invalid id");
            }
        }
    }
}