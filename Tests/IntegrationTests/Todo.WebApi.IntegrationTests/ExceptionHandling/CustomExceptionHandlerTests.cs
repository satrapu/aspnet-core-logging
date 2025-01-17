namespace Todo.WebApi.ExceptionHandling
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Security.Principal;
    using System.Text.Json;
    using System.Threading.Tasks;

    using ApplicationFlows.Security;

    using Autofac;

    using Commons.Constants;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    using Models;

    using NUnit.Framework;

    using Services.Security;

    using TestInfrastructure;

    /// <summary>
    /// Contains integration tests targeting <see cref="CustomExceptionHandler"/> class.
    /// </summary>
    [TestFixture]
    public class CustomExceptionHandlerTests
    {
        /// <summary>
        /// Ensures <see cref="CustomExceptionHandler.HandleException"/> method successfully converts an API exception
        /// to an instance of the <see cref="ProblemDetails"/> class.
        /// </summary>
        [Test]
        public async Task HandleException_WhenApiThrowsException_MustConvertExceptionToInstanceOfProblemDetailsClass()
        {
            // Arrange
            await using TestWebApplicationFactory webApplicationFactory = await TestWebApplicationFactory.CreateAsync
            (
                applicationName: nameof(CustomExceptionHandlerTests),
                environmentName: EnvironmentNames.IntegrationTests,
                shouldRunStartupLogicTasks: false
            );

            webApplicationFactory
                .WithMockServices(containerBuilder =>
                {
                    // Ensure a mock implementation will be injected whenever a service requires an instance of the
                    // IGenerateJwtFlow interface.
                    containerBuilder
                        .RegisterType<GenerateJwtFlowWhichThrowsException>()
                        .As<IGenerateJwtFlow>()
                        .InstancePerLifetimeScope();
                })
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.ConfigureAppConfiguration(configurationBuilder =>
                    {
                        configurationBuilder.AddInMemoryCollection
                        (
                            initialData:
                            [
                                // Ensure database is not migrated, since having an up-to-date RDBMS would just complicate this test method.
                                new KeyValuePair<string, string>("MigrateDatabase", bool.FalseString)
                            ]
                        );
                    });
                });

            GenerateJwtModel generateJwtModel = new()
            {
                UserName = $"test-user--{Guid.NewGuid():N}",
                Password = $"test-password--{Guid.NewGuid():N}"
            };

            using HttpClient httpClient = webApplicationFactory.CreateClient();

            // Act
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("api/jwt", generateJwtModel);

            // Assert
            using AssertionScope _ = new();
            const HttpStatusCode expectedStatusCode = HttpStatusCode.InternalServerError;

            httpResponseMessage.IsSuccessStatusCode.Should().BeFalse("the endpoint was supposed to throw a hard-coded exception");
            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode, $"the hard-coded exception was mapped to HTTP status {expectedStatusCode}");

            byte[] problemDetailsAsBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync();
            await using MemoryStream memoryStream = new(problemDetailsAsBytes);

            // Must use System.Text.Json.JsonSerializer instead of Newtonsoft.Json.JsonSerializer to ensure
            // ProblemDetails.Extensions property is correctly deserialized and does not end up as an empty
            // dictionary.
            ProblemDetails problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(memoryStream);
            problemDetails.Should().NotBeNull("application must handle any exception");
            problemDetails.Detail.Should().NotBeNullOrWhiteSpace("an error is expected");
            problemDetails.Extensions.Should().NotBeNull("problem details must contain extra info");
            problemDetails.Extensions.Should().NotContainKey("errorData", "error data must not be present");
            problemDetails.Extensions.Should().ContainKey("rootCauseKey", "root cause key must be present");
            problemDetails.Extensions.Should().ContainKey("errorId", "error id must be present");

            Guid.TryParse(problemDetails.Extensions["errorId"].ToString(), out Guid _).Should().BeTrue("error id is a GUID");
        }

        /// <summary>
        /// An <see cref="IGenerateJwtFlow"/> implementation which throws an exception whenever its method is called.
        /// </summary>
        private class GenerateJwtFlowWhichThrowsException : IGenerateJwtFlow
        {
            public Task<JwtInfo> ExecuteAsync(GenerateJwtInfo input, IPrincipal flowInitiator)
            {
                throw new InvalidOperationException("This method must fail for testing purposes");
            }
        }
    }
}
