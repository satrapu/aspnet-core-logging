using FluentAssertions.Execution;

using VerifyNUnit;

using VerifyTests;

namespace Todo.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Commons.Constants;

    using FluentAssertions;

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
            testWebApplicationFactory = await TestWebApplicationFactory.CreateAsync
            (
                applicationName: nameof(TodoControllerTests),
                environmentName: EnvironmentNames.IntegrationTests,
                shouldRunStartupLogicTasks: true
            );

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
        public async Task CreateAsync_WhenRequestIsNotAuthorized_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            NewTodoItemModel newTodoItemModel = new()
            {
                Name = null,
                IsComplete = null
            };

            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            // Act
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);

            // Assert
            await Verifier.Verify(response, settings: ModuleInitializer.VerifySettings);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.CreateAsync" /> creates a new entity and returns its identifier.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CreateAsync_UsingValidTodoItemReturnsExpectedResult()
        {
            // Arrange
            NewTodoItemModel newTodoItemModel = new()
            {
                Name = $"it--{Guid.NewGuid():D}",
                IsComplete = true
            };

            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            // Act
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);

            // Assert
            await Verifier.Verify(response, settings: ModuleInitializer.VerifySettings);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.CreateAsync" /> fails in case the provided input is not valid.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CreateAsync_UsingInvalidTodoItem_ReturnsExpectedResult()
        {
            // Arrange
            NewTodoItemModel invalidModel = new();
            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            // Act
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, invalidModel);

            // Assert
            await Verifier.Verify(response, settings: ModuleInitializer.VerifySettings);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByQueryAsync" /> fails in case the current request is not
        /// accompanied by a JWT.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByQueryAsync_WhenRequestIsNotAuthorized_ReturnsExpectedResult()
        {
            // Arrange
            Dictionary<string, string> queryString = new()
            {
                [nameof(TodoItemQueryModel.PageIndex)] = 0.ToString(),
                [nameof(TodoItemQueryModel.PageSize)] = 5.ToString(),
                [nameof(TodoItemQueryModel.SortBy)] = nameof(TodoItem.CreatedOn),
                [nameof(TodoItemQueryModel.IsSortAscending)] = bool.FalseString
            };

            string requestUri = QueryHelpers.AddQueryString(BaseUrl, queryString);
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync(requestUri);

            // Assert
            await Verifier.Verify(response, settings: ModuleInitializer.VerifySettings);
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
            VerifySettings verifySettings = new(ModuleInitializer.VerifySettings);
            verifySettings.ScrubMember("id");

            NewTodoItemModel newTodoItemModel = new()
            {
                Name = $"it--{Guid.NewGuid():D}",
                IsComplete = false,
            };

            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();
            using HttpResponseMessage createTodoItemResponse = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);

            using AssertionScope _ = new();
            createTodoItemResponse.IsSuccessStatusCode.Should().BeTrue();
            createTodoItemResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            createTodoItemResponse.Headers.Location.Should().NotBeNull();

            long id = GetTodoItemIdFrom(createTodoItemResponse);

            Dictionary<string, string> queryString = new()
            {
                [nameof(TodoItemQueryModel.Id)] = id.ToString(),
                [nameof(TodoItemQueryModel.IsComplete)] = newTodoItemModel.IsComplete.ToString(),
                [nameof(TodoItemQueryModel.NamePattern)] = newTodoItemModel.Name,
                [nameof(TodoItemQueryModel.PageIndex)] = 0.ToString(),
                [nameof(TodoItemQueryModel.PageSize)] = 5.ToString(),
                [nameof(TodoItemQueryModel.SortBy)] = nameof(TodoItem.CreatedOn),
                [nameof(TodoItemQueryModel.IsSortAscending)] = bool.FalseString
            };

            string requestUri = QueryHelpers.AddQueryString(BaseUrl, queryString);

            // Act
            using HttpResponseMessage getTodoItemsResponse = await httpClient.GetAsync(requestUri);

            // Assert
            await Verifier.Verify(getTodoItemsResponse, settings: verifySettings);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByIdAsync" /> fails in case the current request is not
        /// accompanied by a JWT.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByIdAsync_WhenRequestIsNotAuthorized_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            long? id = int.MaxValue;
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            // Act
            using HttpResponseMessage response = await httpClient.GetAsync($"{BaseUrl}/{id}");

            // Assert
            await Verifier.Verify(response, settings: ModuleInitializer.VerifySettings);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByIdAsync" /> is able to find a newly created entity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByIdAsync_UsingNewlyCreatedItem_ReturnsExpectedResult()
        {
            // Arrange
            VerifySettings verifySettings = new(ModuleInitializer.VerifySettings);
            verifySettings.ScrubMember("id");

            NewTodoItemModel newTodoItemModel = new()
            {
                Name = $"it--{Guid.NewGuid():D}",
                IsComplete = true,
            };

            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();
            using HttpResponseMessage createTodoItemResponse = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);

            using AssertionScope _ = new();
            createTodoItemResponse.IsSuccessStatusCode.Should().BeTrue();
            createTodoItemResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            createTodoItemResponse.Headers.Location.Should().NotBeNull();

            long id = GetTodoItemIdFrom(createTodoItemResponse);

            // Act
            using HttpResponseMessage getTodoItemResponse = await httpClient.GetAsync($"{BaseUrl}/{id}");

            // Assert
            await Verifier.Verify(getTodoItemResponse, settings: verifySettings);
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
            NewTodoItemModel newTodoItemModel = new()
            {
                Name = $"it--{Guid.NewGuid():D}",
                IsComplete = true
            };

            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();

            using HttpResponseMessage createTodoItemResponse = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);
            createTodoItemResponse.EnsureSuccessStatusCode();

            long nonExistentId = long.MinValue;

            // Act
            using HttpResponseMessage getTodoItemResponse = await httpClient.GetAsync($"{BaseUrl}/{nonExistentId}");

            // Assert
            await Verifier.Verify(getTodoItemResponse, settings: ModuleInitializer.VerifySettings);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.UpdateAsync" /> fails in case the current request is not
        /// accompanied by a JWT.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task UpdateAsync_WhenRequestIsNotAuthorized_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            UpdateTodoItemModel updateTodoItemModel = new();
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            // Act
            using HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{BaseUrl}/{int.MaxValue}", updateTodoItemModel);

            // Assert
            await Verifier.Verify(response, settings: ModuleInitializer.VerifySettings);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.UpdateAsync" /> successfully updates a newly created entity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task UpdateAsync_UsingNewlyCreatedTodoItem_MustSucceed()
        {
            // Arrange
            NewTodoItemModel newTodoItemInfo = new()
            {
                Name = $"it--{Guid.NewGuid():D}",
                IsComplete = true
            };

            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();
            using HttpResponseMessage createTodoItemResponse = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemInfo);

            using AssertionScope _ = new();
            createTodoItemResponse.IsSuccessStatusCode.Should().BeTrue();
            createTodoItemResponse.Headers.Location.Should().NotBeNull();

            UpdateTodoItemModel updateTodoItemModel = new()
            {
                IsComplete = !newTodoItemInfo.IsComplete,
                Name = $"CHANGED--{newTodoItemInfo.Name}"
            };

            long id = GetTodoItemIdFrom(createTodoItemResponse);

            // Act
            using HttpResponseMessage updateTodoItemResponse = await httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateTodoItemModel);

            // Assert
            await Verifier.Verify(updateTodoItemResponse, settings: ModuleInitializer.VerifySettings);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.DeleteAsync" /> fails in case the current request is not
        /// accompanied by a JWT.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DeleteAsync_WhenRequestIsNotAuthorized_ReturnsUnauthorizedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = testWebApplicationFactory.CreateClient();

            // Act
            using HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{int.MaxValue}");

            // Assert
            await Verifier.Verify(response, settings: ModuleInitializer.VerifySettings);
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.DeleteAsync" /> successfully deletes a newly created entity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DeleteAsync_UsingNewlyCreatedTodoItem_MustSucceed()
        {
            // Arrange
            NewTodoItemModel newTodoItemInfo = new()
            {
                Name = $"it--{Guid.NewGuid():D}",
                IsComplete = true
            };

            using HttpClient httpClient = await testWebApplicationFactory.CreateHttpClientAsync();
            using HttpResponseMessage createTodoItemResponse = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemInfo);

            using AssertionScope _ = new();
            createTodoItemResponse.IsSuccessStatusCode.Should().BeTrue();
            createTodoItemResponse.Headers.Location.Should().NotBeNull();

            long? id = GetTodoItemIdFrom(createTodoItemResponse);

            // Act
            using HttpResponseMessage deleteTodoItemResponse = await httpClient.DeleteAsync($"{BaseUrl}/{id}");

            // Assert
            await Verifier.Verify(deleteTodoItemResponse, settings: ModuleInitializer.VerifySettings);
        }

        private static long GetTodoItemIdFrom(HttpResponseMessage httpResponseMessage)
        {
            string locationAsString = httpResponseMessage.Headers.Location?.ToString();
            ArgumentException.ThrowIfNullOrWhiteSpace(locationAsString);

            string todoItemIdAsString = locationAsString.Remove
            (
                startIndex: 0,
                count: locationAsString.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1
            );

            if (int.TryParse(todoItemIdAsString, out int todoItemId))
            {
                return todoItemId;
            }

            throw new FormatException(message: $"Could not extract todo item ID from HTTP response header value: \"{locationAsString}\"");
        }
    }
}
