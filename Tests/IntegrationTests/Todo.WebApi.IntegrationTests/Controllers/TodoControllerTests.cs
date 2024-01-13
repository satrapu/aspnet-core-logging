using Microsoft.Extensions.DependencyInjection;

using Todo.Commons.Constants;
using Todo.Commons.StartupLogic;

namespace Todo.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.AspNetCore.WebUtilities;

    using Models;

    using NUnit.Framework;

    using Persistence.Entities;

    using TestInfrastructure;

    /// <summary>
    ///  Contains integration tests targeting <see cref="TodoController" /> class.
    ///  <br/>
    ///  Based on: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0#aspnet-core-integration-tests.
    ///  and: https://medium.com/@daniel.edwards_82928/using-webapplicationfactory-with-nunit-817a616e26f9.
    /// </summary>
    [TestFixture]
    public class TodoControllerTests
    {
        private const string BaseUrl = "api/todo";

        private TestWebApplicationFactory testWebApplicationFactory;
        private ActivityListener activityListener;
        private ActivityListener noOpActivityListener;

        /// <summary>
        /// Ensures the appropriate <see cref="TestWebApplicationFactory"/> instance has been created before running
        /// any test method found in this class.
        /// </summary>
        [OneTimeSetUp]
        public async Task GivenAnHttpRequestIsToBePerformed()
        {
            testWebApplicationFactory = new TestWebApplicationFactory
            (
                applicationName: nameof(TodoControllerTests),
                environmentName: EnvironmentNames.IntegrationTests
            );

            // Ensure startup logic is executed before running any tests.
            await testWebApplicationFactory.Services.GetRequiredService<IStartupLogicTaskExecutor>().ExecuteAsync();

            activityListener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = _ => { },
                ActivityStopped = _ => { }
            };

            noOpActivityListener = new ActivityListener
            {
                ShouldListenTo = _ => false,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.None,
                ActivityStarted = _ => { },
                ActivityStopped = _ => { }
            };

            ActivitySource.AddActivityListener(activityListener);
            ActivitySource.AddActivityListener(noOpActivityListener);
        }

        /// <summary>
        /// Ensure the <see cref="TestWebApplicationFactory"/> instance is properly disposed after all test methods
        /// found inside this class have been run.
        /// </summary>
        [OneTimeTearDown]
        public void Cleanup()
        {
            testWebApplicationFactory?.Dispose();
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.CreateAsync" /> fails in case the current request is not
        /// accompanied by a JWT.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CreateAsync_UsingNoJsonWebToken_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            NewTodoItemModel newTodoItemModel = new()
            {
                Name = null,
                IsComplete = null
            };

            // Act
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.CreateAsync" /> creates a new entity and returns its identifier.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CreateAsync_UsingValidTodoItem_ReturnsExpectedData()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            const long idThreshold = 1;

            NewTodoItemModel newTodoItemModel = new()
            {
                Name = $"it--{nameof(CreateAsync_UsingValidTodoItem_ReturnsExpectedData)}--{Guid.NewGuid():N}",
                IsComplete = DateTime.UtcNow.Ticks % 2 == 0
            };

            // Act
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().MatchRegex(@"api/todo/\d+");

            long id = GetCreatedEntityIdFrom(response);
            id.Should().BeGreaterOrEqualTo(idThreshold);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.CreateAsync" /> fails in case the provided input is not valid.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CreateAsync_UsingInvalidTodoItem_ReturnsExpectedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();
            NewTodoItemModel invalidModel = new();

            // Act
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, invalidModel);

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByQueryAsync" /> fails in case the current request is not
        /// accompanied by a JWT.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByQueryAsync_UsingNoJsonWebToken_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            Dictionary<string, string> queryString = new()
            {
                [nameof(TodoItemQueryModel.PageIndex)] = 0.ToString(),
                [nameof(TodoItemQueryModel.PageSize)] = 5.ToString(),
                [nameof(TodoItemQueryModel.SortBy)] = nameof(TodoItem.CreatedOn),
                [nameof(TodoItemQueryModel.IsSortAscending)] = bool.FalseString
            };

            string requestUri = QueryHelpers.AddQueryString(BaseUrl, queryString);

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync(requestUri);

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByQueryAsync" /> returns the expected outcome
        /// when using a valid query.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByQueryAsync_UsingDefaults_ReturnsExpectedResult()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            string nameSuffix = Guid.NewGuid().ToString("N");
            string name = $"it--{nameof(GetByQueryAsync_UsingDefaults_ReturnsExpectedResult)}--{nameSuffix}";

            NewTodoItemModel newTodoItemModel = new()
            {
                IsComplete = DateTime.UtcNow.Ticks % 2 == 0,
                Name = name
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            long id = GetCreatedEntityIdFrom(response);

            Dictionary<string, string> queryString = new()
            {
                [nameof(TodoItemQueryModel.Id)] = id.ToString(),
                [nameof(TodoItemQueryModel.IsComplete)] = newTodoItemModel.IsComplete.ToString(),
                [nameof(TodoItemQueryModel.NamePattern)] = name,
                [nameof(TodoItemQueryModel.PageIndex)] = 0.ToString(),
                [nameof(TodoItemQueryModel.PageSize)] = 5.ToString(),
                [nameof(TodoItemQueryModel.SortBy)] = nameof(TodoItem.CreatedOn),
                [nameof(TodoItemQueryModel.IsSortAscending)] = bool.FalseString
            };

            string requestUri = QueryHelpers.AddQueryString(BaseUrl, queryString);

            // Act
            response = await httpClient.GetAsync(requestUri);

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<TodoItemModel> todoItemModels = await response.Content.ReadAsAsync<List<TodoItemModel>>();
            todoItemModels.Should().HaveCount(1);

            TodoItemModel todoItemModel = todoItemModels.SingleOrDefault();
            todoItemModel.Should().NotBeNull();
            todoItemModel!.Name.Should().Be(newTodoItemModel.Name);
            todoItemModel!.IsComplete.Should().Be(newTodoItemModel.IsComplete.Value);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByIdAsync" /> fails in case the current request is not
        /// accompanied by a JWT.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByIdAsync_UsingNoJsonWebToken_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();
            long? id = int.MaxValue;

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync($"{BaseUrl}/{id}");

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByIdAsync" /> is able to find a newly created entity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByIdAsync_UsingNewlyCreatedItem_ReturnsExpectedResult()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            string nameSuffix = Guid.NewGuid().ToString("N");
            string name = $"it--{nameof(GetByIdAsync_UsingNewlyCreatedItem_ReturnsExpectedResult)}--{nameSuffix}";

            NewTodoItemModel newTodoItemModel = new()
            {
                IsComplete = DateTime.UtcNow.Ticks % 2 == 0,
                Name = name
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            long id = GetCreatedEntityIdFrom(response);

            // Act
            response = await httpClient.GetAsync($"{BaseUrl}/{id}");

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            TodoItemModel todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>();
            todoItemModel.Should().NotBeNull();
            todoItemModel.Id.Should().Be(id);
            todoItemModel.Name.Should().Be(newTodoItemModel.Name);
            todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete.Value);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByIdAsync" /> fails in case the API does not find any entities
        /// matching the given input query.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByIdAsync_UsingNonExistingId_ReturnsNotFoundHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            string nameSuffix = Guid.NewGuid().ToString("N");
            string name = $"it--{nameof(GetByIdAsync_UsingNonExistingId_ReturnsNotFoundHttpStatusCode)}--{nameSuffix}";

            NewTodoItemModel newTodoItemModel = new()
            {
                IsComplete = DateTime.UtcNow.Ticks % 2 == 0,
                Name = name
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);
            response.EnsureSuccessStatusCode();

            long nonExistentId = long.MinValue;

            // Act
            response = await httpClient.GetAsync($"{BaseUrl}/{nonExistentId}");

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.UpdateAsync" /> fails in case the current request is not
        /// accompanied by a JWT.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task UpdateAsync_UsingNoJsonWebToken_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();
            long? id = int.MaxValue;
            UpdateTodoItemModel updateTodoItemModel = new();

            // Act
            using HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateTodoItemModel);

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.UpdateAsync" /> successfully updates a newly created entity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task UpdateAsync_UsingNewlyCreatedTodoItem_MustSucceed()
        {
            // Arrange
            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            string name = $"it--{nameof(UpdateAsync_UsingNewlyCreatedTodoItem_MustSucceed)}--{Guid.NewGuid():N}";
            bool isComplete = DateTime.UtcNow.Ticks % 2 == 0;

            NewTodoItemModel newTodoItemInfo = new()
            {
                Name = name,
                IsComplete = isComplete
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemInfo);
            response.IsSuccessStatusCode.Should().BeTrue();
            response.Headers.Location.Should().NotBeNull();

            long id = GetCreatedEntityIdFrom(response);

            UpdateTodoItemModel updateTodoItemModel = new()
            {
                IsComplete = !newTodoItemInfo.IsComplete,
                Name = $"CHANGED--{newTodoItemInfo.Name}"
            };

            // Act
            response = await httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateTodoItemModel);

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            response = await httpClient.GetAsync($"{BaseUrl}/{id}");
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            TodoItemModel todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>();
            todoItemModel.Should().NotBeNull();
            todoItemModel.Id.Should().Be(id);
            todoItemModel.IsComplete.Should().Be(updateTodoItemModel.IsComplete.Value);
            todoItemModel.Name.Should().Be(updateTodoItemModel.Name);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.DeleteAsync" /> fails in case the current request is not
        /// accompanied by a JWT.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DeleteAsync_UsingNoJsonWebToken_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();
            long id = int.MaxValue;

            // Act
            using HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.DeleteAsync" /> successfully deletes a newly created entity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DeleteAsync_UsingNewlyCreatedTodoItem_MustSucceed()
        {
            // Arrange
            string name = $"it--{nameof(DeleteAsync_UsingNewlyCreatedTodoItem_MustSucceed)}--{Guid.NewGuid():N}";
            bool isComplete = DateTime.UtcNow.Ticks % 2 == 0;

            NewTodoItemModel newTodoItemInfo = new()
            {
                Name = name,
                IsComplete = isComplete
            };

            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemInfo);
            response.IsSuccessStatusCode.Should().BeTrue();
            response.Headers.Location.Should().NotBeNull();

            long? id = GetCreatedEntityIdFrom(response);

            // Act
            response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");

            // Assert
            using AssertionScope _ = new();
            response.IsSuccessStatusCode.Should().BeTrue();

            response = await httpClient.GetAsync($"{BaseUrl}/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static long GetCreatedEntityIdFrom(HttpResponseMessage httpResponseMessage)
        {
            ArgumentNullException.ThrowIfNull(httpResponseMessage.Headers.Location);

            string locationAsString = httpResponseMessage.Headers.Location.ToString();
            string createdEntityIdAsString = locationAsString.Remove
            (
                startIndex: 0,
                count: locationAsString.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1
            );

            return int.Parse(createdEntityIdAsString);
        }
    }
}
