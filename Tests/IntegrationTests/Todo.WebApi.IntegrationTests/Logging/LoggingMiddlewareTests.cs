using FluentAssertions;
using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Todo.WebApi.Infrastructure;

namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains integration tests targeting <see cref="LoggingMiddleware"/> class.
    /// </summary>
    [TestFixture]
    public class LoggingMiddlewareTests
    {
        private TodoWebApplicationFactory testFactory;

        [OneTimeSetUp]
        public void GivenAnHttpRequestIsToBePerformed()
        {
            testFactory = new TodoWebApplicationFactory();
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_WithTextHeaderNotTriggeringRequestBeingLogged_MustFail()
        {
            // Arrange
            using (var client = testFactory.CreateClient())
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "qwerty/123456");
                httpRequestMessage.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("qwerty/123456"));

                // Act
                var httpResponseMessage = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                // Assert
                httpResponseMessage.IsSuccessStatusCode.Should().BeFalse();
            }
        }

        /// <summary>
        /// Tests <see cref="LoggingMiddleware.Invoke"/> method.
        /// </summary>
        [Test]
        public async Task Invoke_WithTextHeaderTriggeringRequestBeingLogged_MustSucceed()
        {
            // Arrange
            using (var client = testFactory.CreateClient())
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/todo");
                httpRequestMessage.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                // Act
                var httpResponseMessage = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                // Assert
                httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
            }
        }
    }
}
