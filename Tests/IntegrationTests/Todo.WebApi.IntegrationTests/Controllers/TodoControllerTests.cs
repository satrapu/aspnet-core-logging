namespace Todo.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.AspNetCore.WebUtilities;

    using Models;

    using NUnit.Framework;

    using Persistence.Entities;

    using TestInfrastructure;

    /// <summary>
    ///  Contains integration tests targeting <seealso cref="TodoController" /> class.
    ///  <br/>
    ///  Based on: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.1#aspnet-core-integration-tests.
    ///  and: https://medium.com/@daniel.edwards_82928/using-webapplicationfactory-with-nunit-817a616e26f9.
    /// </summary>
    [TestFixture]
    public class TodoControllerTests
    {
        private TestWebApplicationFactory webApplicationFactory;
        private const string BaseUrl = "api/todo";
        private const string BecauseCurrentRequestHasNoJwt = "the request does not contain a JSON web token";
        private const string BecauseAnEntityHasBeenCreated = "an entity has been created";
        private const string BecauseCleanupMustSucceed = "cleanup must succeed";
        private const string BecauseInputModelIsInvalid = "input model is invalid";
        private const string BecauseNewEntityHasBeenCreated = "a new entity has been created";
        private const string BecauseEntityHasBeenPreviouslyCreated = "an entity has been previously created";
        private const string BecauseMustNotFindSomethingWhichDoesNotExist = "must not find something which does not exist";

        /// <summary>
        /// Ensures the appropriate <see cref="TestWebApplicationFactory"/> instance has been created before running
        /// any test method found in this class.
        /// </summary>
        [OneTimeSetUp]
        public void GivenAnHttpRequestIsToBePerformed()
        {
            webApplicationFactory = new TestWebApplicationFactory(MethodBase.GetCurrentMethod()?.DeclaringType?.Name);
        }

        /// <summary>
        /// Ensure the <see cref="TestWebApplicationFactory"/> instance is properly disposed after all test methods
        /// found inside this class have been run.
        /// </summary>
        [OneTimeTearDown]
        public void Cleanup()
        {
            webApplicationFactory?.Dispose();
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
            using HttpClient httpClient = webApplicationFactory.CreateClient();
            var newTodoItemModel = new NewTodoItemModel();

            // Act
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);

            // Assert
            using (new AssertionScope())
            {
                response.IsSuccessStatusCode.Should().BeFalse(BecauseCurrentRequestHasNoJwt);
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, BecauseCurrentRequestHasNoJwt);
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.CreateAsync" /> creates a new entity and returns its identifier.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CreateAsync_UsingValidTodoItem_ReturnsExpectedData()
        {
            // Arrange
            using HttpClient httpClient = await webApplicationFactory.CreateClientWithJwtAsync();
            long? id = null;
            const long idThreshold = 1;

            try
            {
                var newTodoItemModel = new NewTodoItemModel
                {
                    Name = $"it--{nameof(CreateAsync_UsingValidTodoItem_ReturnsExpectedData)}--{Guid.NewGuid():N}",
                    IsComplete = DateTime.UtcNow.Ticks % 2 == 0
                };

                // Act
                HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemModel);

                // Assert
                using (new AssertionScope())
                {
                    response.IsSuccessStatusCode.Should().BeTrue(BecauseAnEntityHasBeenCreated);
                    response.StatusCode.Should().Be(HttpStatusCode.Created, BecauseAnEntityHasBeenCreated);
                    response.Headers.ToDictionary(x => x.Key, x => x.Value).Should().ContainKey("Location");
                    response.Headers.Location?.OriginalString
                        .Should().MatchRegex(@"api/todo/\d+", BecauseAnEntityHasBeenCreated);

                    id = await response.Content.ReadAsAsync<long>();
                    id.Should().BeGreaterOrEqualTo(idThreshold, BecauseAnEntityHasBeenCreated);
                }
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue(BecauseCleanupMustSucceed);
                }
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.CreateAsync" /> fails in case the provided input is not valid.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CreateAsync_UsingInvalidTodoItem_ReturnsExpectedHttpStatusCode()
        {
            // Arrange
            using HttpClient httpClient = await webApplicationFactory.CreateClientWithJwtAsync();
            var invalidModel = new NewTodoItemModel();

            // Act
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, invalidModel);

            // Assert
            using (new AssertionScope())
            {
                response.IsSuccessStatusCode.Should().BeFalse(BecauseInputModelIsInvalid);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity, BecauseInputModelIsInvalid);
            }
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
            using HttpClient httpClient = webApplicationFactory.CreateClient();

            var queryString = new Dictionary<string, string>
            {
                {nameof(TodoItemQueryModel.PageIndex), 0.ToString()},
                {nameof(TodoItemQueryModel.PageSize), 5.ToString()},
                {nameof(TodoItemQueryModel.SortBy), nameof(TodoItem.CreatedOn)},
                {nameof(TodoItemQueryModel.IsSortAscending), bool.FalseString}
            };
            string requestUri = QueryHelpers.AddQueryString(BaseUrl, queryString);

            // Act
            HttpResponseMessage response = await httpClient.GetAsync(requestUri);

            // Assert
            using (new AssertionScope())
            {
                response.IsSuccessStatusCode.Should().BeFalse(BecauseCurrentRequestHasNoJwt);
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, BecauseCurrentRequestHasNoJwt);
            }
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
            using HttpClient httpClient = await webApplicationFactory.CreateClientWithJwtAsync();
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
                using (new AssertionScope())
                {
                    response.IsSuccessStatusCode.Should().BeTrue();
                    response.StatusCode.Should().Be(HttpStatusCode.OK);

                    List<TodoItemModel> todoItemModels = await response.Content.ReadAsAsync<List<TodoItemModel>>();
                    todoItemModels.Should().HaveCount(1);

                    TodoItemModel todoItemModel = todoItemModels.Single();
                    todoItemModel.Should().NotBeNull();
                    todoItemModel.Name.Should().Be(newTodoItemModel.Name);
                    todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete.Value);
                }
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue(BecauseCleanupMustSucceed);
                }
            }
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
            using HttpClient httpClient = webApplicationFactory.CreateClient();
            long? id = int.MaxValue;

            // Act
            HttpResponseMessage response = await httpClient.GetAsync($"{BaseUrl}/{id}");

            // Assert
            using (new AssertionScope())
            {
                response.IsSuccessStatusCode.Should().BeFalse(BecauseCurrentRequestHasNoJwt);
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, BecauseCurrentRequestHasNoJwt);
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.GetByIdAsync" /> is able to find a newly created entity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetByIdAsync_UsingNewlyCreatedItem_ReturnsExpectedResult()
        {
            // Arrange
            using HttpClient httpClient = await webApplicationFactory.CreateClientWithJwtAsync();
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
                using (new AssertionScope())
                {
                    response.IsSuccessStatusCode.Should().BeTrue();
                    response.StatusCode.Should().Be(HttpStatusCode.OK);

                    TodoItemModel todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>();
                    todoItemModel.Should().NotBeNull();
                    todoItemModel.Id.Should().Be(id);
                    todoItemModel.Name.Should().Be(newTodoItemModel.Name);
                    todoItemModel.IsComplete.Should().Be(newTodoItemModel.IsComplete.Value);
                }
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue(BecauseCleanupMustSucceed);
                }
            }
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
            using HttpClient httpClient = await webApplicationFactory.CreateClientWithJwtAsync();
            long? id = null;

            try
            {
                string nameSuffix = Guid.NewGuid().ToString("N");
                string name =
                    $"it--{nameof(GetByIdAsync_UsingNonExistingId_ReturnsNotFoundHttpStatusCode)}--{nameSuffix}";

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
                using (new AssertionScope())
                {
                    response.IsSuccessStatusCode.Should().BeFalse(BecauseMustNotFindSomethingWhichDoesNotExist);
                    response.StatusCode.Should()
                        .Be(HttpStatusCode.NotFound, BecauseMustNotFindSomethingWhichDoesNotExist);
                }
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue(BecauseCleanupMustSucceed);
                }
            }
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
            using HttpClient httpClient = webApplicationFactory.CreateClient();
            long? id = int.MaxValue;
            var updateTodoItemModel = new UpdateTodoItemModel();

            // Act
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateTodoItemModel);

            // Assert
            using (new AssertionScope())
            {
                response.IsSuccessStatusCode.Should().BeFalse(BecauseCurrentRequestHasNoJwt);
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, BecauseCurrentRequestHasNoJwt);
            }
        }

        /// <summary>
        /// Ensures method <see cref="TodoController.UpdateAsync" /> successfully updates a newly created entity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task UpdateAsync_UsingNewlyCreatedTodoItem_MustSucceed()
        {
            // Arrange
            using HttpClient httpClient = await webApplicationFactory.CreateClientWithJwtAsync();
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
                response.IsSuccessStatusCode.Should().BeTrue(BecauseNewEntityHasBeenCreated);
                id = await response.Content.ReadAsAsync<long>();

                var updateTodoItemModel = new UpdateTodoItemModel
                {
                    IsComplete = !newTodoItemInfo.IsComplete,
                    Name = $"CHANGED--{newTodoItemInfo.Name}"
                };

                // Act
                response = await httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updateTodoItemModel);

                // Assert
                using (new AssertionScope())
                {
                    response.IsSuccessStatusCode.Should().BeTrue(BecauseEntityHasBeenPreviouslyCreated);
                    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                    response = await httpClient.GetAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue("an entity has been previously updated");
                    response.StatusCode.Should().Be(HttpStatusCode.OK);

                    TodoItemModel todoItemModel = await response.Content.ReadAsAsync<TodoItemModel>();
                    todoItemModel.Should().NotBeNull(BecauseEntityHasBeenPreviouslyCreated);
                    todoItemModel.Id.Should().Be(id);
                    todoItemModel.IsComplete.Should().Be(updateTodoItemModel.IsComplete.Value);
                    todoItemModel.Name.Should().Be(updateTodoItemModel.Name);
                }
            }
            finally
            {
                if (id.HasValue)
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
                    response.IsSuccessStatusCode.Should().BeTrue(BecauseCleanupMustSucceed);
                }
            }
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
            using HttpClient httpClient = webApplicationFactory.CreateClient();
            long? id = int.MaxValue;

            // Act
            HttpResponseMessage response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");

            // Assert
            using (new AssertionScope())
            {
                response.IsSuccessStatusCode.Should().BeFalse(BecauseCurrentRequestHasNoJwt);
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, BecauseCurrentRequestHasNoJwt);
            }
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

            var newTodoItemInfo = new NewTodoItemModel
            {
                Name = name,
                IsComplete = isComplete
            };

            using HttpClient httpClient = await webApplicationFactory.CreateClientWithJwtAsync();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseUrl, newTodoItemInfo);
            response.IsSuccessStatusCode.Should().BeTrue(BecauseNewEntityHasBeenCreated);
            long? id = await response.Content.ReadAsAsync<long>();

            // Act
            response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");

            // Assert
            using (new AssertionScope())
            {
                response.IsSuccessStatusCode.Should().BeTrue("existing entity must be deleted");
                response = await httpClient.GetAsync($"{BaseUrl}/{id}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound, "existing entity has already been deleted");
            }
        }
    }
}
