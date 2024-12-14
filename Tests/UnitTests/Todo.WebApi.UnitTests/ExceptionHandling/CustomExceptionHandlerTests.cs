namespace Todo.WebApi.ExceptionHandling
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Configuration;

    using FluentAssertions;

    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    using Npgsql;

    using NUnit.Framework;

    using Persistence.Entities;

    using Services.TodoItemManagement;

    /// <summary>
    /// Contains unit tests targeting <see cref="CustomExceptionHandler"/> class.
    /// </summary>
    [TestFixture]
    public class CustomExceptionHandlerTests
    {
        [Test]
        [TestCaseSource(nameof(GetExceptions))]
        public async Task HandleException_WhenThereIsAnExceptionToHandle_MustSucceed(Exception exception, bool includeDetails)
        {
            // Arrange
            using MemoryStream memoryStream = new();

            Dictionary<string, string> dictionary = new()
            {
                ["ExceptionHandling:IncludeDetails"] = includeDetails.ToString()
            };

            Mock<IExceptionHandlerFeature> exceptionHandlerFeature = new();
            exceptionHandlerFeature
                .SetupGet(x => x.Error)
                .Returns(exception);

            FeatureCollection featureCollection = new();
            featureCollection.Set(exceptionHandlerFeature.Object);

            IConfiguration configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(dictionary)
                    .Build();

            IServiceProvider serviceProvider =
                new ServiceCollection()
                    .Configure<ExceptionHandlingOptions>(configuration.GetSection("ExceptionHandling"))
                    .AddSingleton<ILoggerFactory, NullLoggerFactory>()
                    .BuildServiceProvider();

            Mock<HttpResponse> httpResponse = new();
            httpResponse
                .SetupGet(x => x.Body)
                .Returns(memoryStream);

            Mock<HttpContext> httpContext = new();
            httpContext
                .SetupGet(x => x.RequestServices)
                .Returns(serviceProvider);

            httpContext
                .SetupGet(x => x.Features)
                .Returns(featureCollection);

            httpContext
                .SetupGet(x => x.Response)
                .Returns(httpResponse.Object);

            // Act
            Func<Task> handleExceptionCall = async () => await CustomExceptionHandler.HandleException(httpContext.Object);

            // Assert
            await handleExceptionCall.Should().NotThrowAsync("there is no exception to handle");
        }

        private static List<object[]> GetExceptions()
        {
            return
            [
                new object[]
                {
                    null, false
                },
                new object[]
                {
                    new EntityNotFoundException(typeof(TodoItem), "test"), true
                },
                new object[]
                {
                    new NpgsqlException(), false
                },
                new object[]
                {
                    new Exception("Hard-coded exception with a cause", new NpgsqlException()), true
                },
                new object[]
                {
                    new Exception("Hard-coded exception"), false
                }
            ];
        }
    }
}
