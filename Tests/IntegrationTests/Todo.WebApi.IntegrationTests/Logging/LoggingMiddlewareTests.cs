using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Todo.WebApi.Infrastructure;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains integration tests targeting <see cref="LoggingMiddleware"/> class.
    /// </summary>
    [TestFixture]
    public class LoggingMiddlewareTests
    {
        private TestWebApplicationFactory testWebApplicationFactory;

        [OneTimeSetUp]
        public void GivenAnHttpRequestIsToBePerformed()
        {
            testWebApplicationFactory =
                new TestWebApplicationFactory(MethodBase.GetCurrentMethod().DeclaringType?.Name);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            testWebApplicationFactory?.Dispose();
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_AgainstUnknownEndpointWithTextHeaderNotTriggeringRequestBeingLogged_MustFail()
        {
            // Arrange
            using HttpClient httpClient =
                await testWebApplicationFactory.CreateClientWithJwtAsync().ConfigureAwait(false);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "qwerty/123456");
            httpRequestMessage.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("qwerty/123456"));

            // Act
            HttpResponseMessage httpResponseMessage =
                await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            // Assert
            httpResponseMessage.IsSuccessStatusCode.Should().BeFalse();
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_AgainstKnownEndpointWithTextHeaderTriggeringRequestBeingLogged_MustSucceed()
        {
            // Arrange
            using HttpClient httpClient =
                await testWebApplicationFactory.CreateClientWithJwtAsync().ConfigureAwait(false);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/todo");
            httpRequestMessage.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage httpResponseMessage =
                await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            // Assert
            httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}