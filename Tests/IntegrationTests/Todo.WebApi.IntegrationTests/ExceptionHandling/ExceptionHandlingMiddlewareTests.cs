using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Todo.Services;
using Todo.WebApi.Infrastructure;

namespace Todo.WebApi.ExceptionHandling
{
    /// <summary>
    /// Contains integration tests targeting <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    [TestFixture]
    public class ExceptionHandlingMiddlewareTests
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
        /// Tests <see cref="ExceptionHandlingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_AgainstEndpointThrowingException_MustHandleException()
        {
            // Arrange
            using HttpClient httpClient =
                await webApplicationFactoryWhichThrowsException.CreateClientWithJwtToken().ConfigureAwait(false);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/todo");
            httpRequestMessage.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage httpResponseMessage =
                await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            // Assert
            httpResponseMessage.IsSuccessStatusCode.Should().BeFalse();
            httpResponseMessage.Headers.TryGetValues("ErrorId", out var values);
            string[] errorIdHeaderValues = values as string[] ?? values.ToArray();
            errorIdHeaderValues.Should().NotBeNullOrEmpty("an unhandled exception was eventually handled");
            errorIdHeaderValues.First().Should()
                .NotBeNullOrWhiteSpace("an id has been associated with the unhandled exception");
        }

        private class WebApplicationFactoryWhichThrowsException : TestWebApplicationFactory
        {
            public WebApplicationFactoryWhichThrowsException() : base(nameof(ExceptionHandlingMiddlewareTests))
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
                throw new System.NotImplementedException();
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