using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Todo.Services;
using Todo.WebApi.Infrastructure;

namespace Todo.WebApi.ExceptionHandling
{
    /// <summary>
    /// Contains integration tests targeting <see cref="CustomExceptionHandlerHelper"/> class.
    /// </summary>
    [TestFixture]
    public class CustomErrorHandlerHelperTests
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
        /// Tests <see cref="CustomExceptionHandlerHelper.WriteResponse"/> method.
        /// </summary>
        [Test]
        public async Task WriteResponse_AgainstEndpointThrowingException_MustHandleException()
        {
            // Arrange
            using HttpClient httpClient =
                await webApplicationFactoryWhichThrowsException.CreateClientWithJwtAsync().ConfigureAwait(false);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/todo");

            // Act
            HttpResponseMessage httpResponseMessage =
                await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            // Assert
            httpResponseMessage.IsSuccessStatusCode.Should()
                .BeFalse("the endpoint was supposed to throw a hard-coded exception");
            httpResponseMessage.StatusCode.Should()
                .Be(HttpStatusCode.NotFound, "the hard-coded exception was mapped to HTTP status 404");
            byte[] problemDetailsAsBytes =
                await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            await using var memoryStream = new MemoryStream(problemDetailsAsBytes);

            // Must use System.Text.Json.JsonSerializer instead of Newtonsoft.Json.JsonSerializer to ensure
            // ProblemDetails.Extensions property is correctly deserialized and does not end up as an empty dictionary
            ProblemDetails problemDetails =
                await JsonSerializer.DeserializeAsync<ProblemDetails>(memoryStream).ConfigureAwait(false);
            problemDetails.Extensions.TryGetValue("errorId", out object errorId).Should()
                .BeTrue("an id must accompany the unhandled exception");
            errorId?.ToString().Should()
                .NotBeNullOrWhiteSpace("the id accompanying the unhandled exception must not be null or whitespace");
        }

        private class WebApplicationFactoryWhichThrowsException : TestWebApplicationFactory
        {
            public WebApplicationFactoryWhichThrowsException() : base(nameof(CustomErrorHandlerHelperTests))
            {
            }

            protected override void ConfigureWebHost(IWebHostBuilder webHostBuilder)
            {
                webHostBuilder.ConfigureTestServices(services =>
                {
                    ServiceDescriptor serviceDescriptor = services.SingleOrDefault(localServiceDescriptor =>
                        localServiceDescriptor.ServiceType == typeof(TodoService));

                    if (serviceDescriptor != null)
                    {
                        services.Remove(serviceDescriptor);
                    }

                    services.AddScoped<ITodoService, TodoServiceWhichThrowsException>();
                });

                base.ConfigureWebHost(webHostBuilder);
            }
        }

        private class TodoServiceWhichThrowsException : ITodoService
        {
            public Task<IList<TodoItemInfo>> GetByQueryAsync(TodoItemQuery todoItemQuery)
            {
                throw new EntityNotFoundException(todoItemQuery.GetType(), null);
            }

            public Task<long> AddAsync(NewTodoItemInfo newTodoItemInfo)
            {
                throw new System.NotImplementedException();
            }

            public Task UpdateAsync(UpdateTodoItemInfo updateTodoItemInfo)
            {
                throw new System.NotImplementedException();
            }

            public Task DeleteAsync(DeleteTodoItemInfo deleteTodoItemInfo)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}