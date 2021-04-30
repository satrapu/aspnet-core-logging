namespace Todo.WebApi.ExceptionHandling
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using Services.TodoItemLifecycleManagement;

    using TestInfrastructure;

    /// <summary>
    /// Contains integration tests targeting <see cref="CustomExceptionHandler"/> class.
    /// </summary>
    [TestFixture]
    public class CustomExceptionHandlerTests
    {
        private WebApplicationFactoryWhichThrowsException webApplicationFactoryWhichThrowsException;

        [OneTimeSetUp]
        public void GivenAnHttpRequestIsToBePerformed()
        {
            webApplicationFactoryWhichThrowsException = new WebApplicationFactoryWhichThrowsException();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            webApplicationFactoryWhichThrowsException?.Dispose();
        }

        /// <summary>
        /// Ensures <see cref="CustomExceptionHandler.HandleException"/> method successfully converts an API exception
        /// to an instance of the <see cref="ProblemDetails"/> class.
        /// </summary>
        [Test]
        public async Task HandleException_WhenApiThrowsException_MustConvertExceptionToInstanceOfProblemDetailsClass()
        {
            // Arrange
            using HttpClient httpClient = await webApplicationFactoryWhichThrowsException.CreateClientWithJwtAsync();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/todo");

            // Act
            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            // Assert
            using (new AssertionScope())
            {
                httpResponseMessage.IsSuccessStatusCode
                    .Should().BeFalse("the endpoint was supposed to throw a hard-coded exception");

                httpResponseMessage.StatusCode
                    .Should().Be(HttpStatusCode.NotFound, "the hard-coded exception was mapped to HTTP status 404");

                byte[] problemDetailsAsBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                await using var memoryStream = new MemoryStream(problemDetailsAsBytes);

                // Must use System.Text.Json.JsonSerializer instead of Newtonsoft.Json.JsonSerializer to ensure
                // ProblemDetails.Extensions property is correctly deserialized and does not end up as an empty
                // dictionary.
                ProblemDetails problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(memoryStream);
                problemDetails.Should().NotBeNull("application must handle any exception");
                // ReSharper disable once PossibleNullReferenceException
                problemDetails.Extensions.Should().NotBeNull("problem details must contain extra info");
                problemDetails.Extensions.Should().NotContainKey("errorData", "error data must not be present");
                problemDetails.Extensions.Should().ContainKey("errorKey", "error key must be present");
                problemDetails.Extensions.Should().ContainKey("errorId", "error id must be present");
                Guid.TryParse(problemDetails.Extensions["errorId"].ToString(), out Guid _)
                    .Should().BeTrue("error id is a GUID");
            }
        }

        /// <summary>
        /// An <see cref="TestWebApplicationFactory"/> which throws an exception whenever one particular service
        /// is called.
        /// </summary>
        private class WebApplicationFactoryWhichThrowsException : TestWebApplicationFactory
        {
            public WebApplicationFactoryWhichThrowsException() : base(nameof(CustomExceptionHandlerTests))
            {
            }

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.ConfigureTestServices(services =>
                {
                    ServiceDescriptor serviceDescriptor = services.SingleOrDefault(localServiceDescriptor =>
                        localServiceDescriptor.ServiceType == typeof(TodoItemService));

                    if (serviceDescriptor != null)
                    {
                        services.Remove(serviceDescriptor);
                    }

                    services.AddScoped<ITodoItemService, TodoItemServiceWhichThrowsException>();
                });

                base.ConfigureWebHost(builder);
            }
        }

        /// <summary>
        /// An <see cref="ITodoItemService"/> implementation which throws an exception when any of its methods
        /// are called.
        /// </summary>
        private class TodoItemServiceWhichThrowsException : ITodoItemService
        {
            public Task<IList<TodoItemInfo>> GetByQueryAsync(TodoItemQuery todoItemQuery)
            {
                var hardCodedException = new EntityNotFoundException(todoItemQuery.GetType(), null);
                hardCodedException.Data.Add("Key1", "Value1");
                hardCodedException.Data.Add("Key2", "Value2");

                throw hardCodedException;
            }

            public Task<long> AddAsync(NewTodoItemInfo newTodoItemInfo)
            {
                throw new NotImplementedException();
            }

            public Task UpdateAsync(UpdateTodoItemInfo updateTodoItemInfo)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(DeleteTodoItemInfo deleteTodoItemInfo)
            {
                throw new NotImplementedException();
            }
        }
    }
}
