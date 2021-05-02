namespace Todo.WebApi.ExceptionHandling
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

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

    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Contains unit tests targeting <see cref="CustomExceptionHandler"/> class.
    /// </summary>
    [TestFixture]
    public class CustomExceptionHandlerTests
    {
        [Test]
        [TestCaseSource(nameof(GetExceptions))]
        public void HandleException_WhenThereIsAnExceptionToHandle_MustSucceed(Exception exception,
            bool includeDetails)
        {
            // Arrange
            var dictionary = new Dictionary<string, string>
            {
                {
                    "ExceptionHandling:IncludeDetails", includeDetails.ToString()
                }
            };

            var configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(dictionary)
                    .Build();

            var exceptionHandlerFeature = new Mock<IExceptionHandlerFeature>();
            exceptionHandlerFeature.SetupGet(x => x.Error).Returns(exception);

            FeatureCollection featureCollection = new FeatureCollection();
            featureCollection.Set(exceptionHandlerFeature.Object);

            var serviceCollection = new ServiceCollection();
            // ReSharper disable once SettingNotFoundInConfiguration
            serviceCollection.Configure<ExceptionHandlingOptions>(configuration.GetSection("ExceptionHandling"));
            serviceCollection.AddSingleton<ILoggerFactory, NullLoggerFactory>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            using var memoryStream = new MemoryStream();

            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupGet(x => x.Body).Returns(memoryStream);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(x => x.RequestServices).Returns(serviceProvider);
            httpContext.SetupGet(x => x.Features).Returns(featureCollection);
            httpContext.SetupGet(x => x.Response).Returns(httpResponse.Object);

            // Act
            Func<Task> handleExceptionCall =
                async () => await CustomExceptionHandler.HandleException(httpContext.Object);

            // Assert
            handleExceptionCall.Should().NotThrow("there is no exception to handle");
        }

        private static IEnumerable<object[]> GetExceptions()
        {
            return new List<object[]>
            {
                new object[]{ null, false },
                new object[]{ new EntityNotFoundException(typeof(TodoItem), "test"), true },
                new object[]{ new NpgsqlException(), false },
                new object[]{ new Exception("Hard-coded exception with a cause", new NpgsqlException()), true },
                new object[]{ new Exception("Hard-coded exception"), false }
            };
        }
    }
}
