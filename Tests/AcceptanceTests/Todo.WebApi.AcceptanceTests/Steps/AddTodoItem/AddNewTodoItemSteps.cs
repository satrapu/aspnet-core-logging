namespace Todo.WebApi.AcceptanceTests.Steps.AddTodoItem
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Drivers;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Assist;

    [Binding]
    public class AddNewTodoItemSteps
    {
        private readonly TodoWebApiDriver todoWebApiDriver;
        private NewTodoItemInfo newTodoItemInfo;
        private AuthenticationHeaderValue authenticationHeaderValue;
        private HttpResponseMessage httpResponseMessage;

        public AddNewTodoItemSteps(TodoWebApiDriver todoWebApiDriver)
        {
            this.todoWebApiDriver = todoWebApiDriver;
        }

        [Given("the current user is authorized to add todo items")]
        public async Task GivenTheCurrentUserIsAuthorizedToAddTodoItems()
        {
            UserDetails userDetails = new(UserName: "acceptance-tests", Password: "Qwerty_123!");
            authenticationHeaderValue = await todoWebApiDriver.GetAuthorizationHeaderAsync(userDetails);
        }

        [Given("the current user is not authorized to add todo items")]
        public async Task GivenTheCurrentUserIsNotAuthorizedToAddTodoItems()
        {
            UserDetails unauthorizedUser = new(UserName: "unauthorized-user-tests", Password: Guid.NewGuid().ToString("N"));
            authenticationHeaderValue =
                await todoWebApiDriver.GetAuthorizationHeaderAsync(unauthorizedUser, scopes: new[] { $"dummy-scope-{Guid.NewGuid():N}" });
        }

        [Given("the current user is not authenticated")]
        public void GivenTheCurrentUserIsNotAuthenticated()
        {
            authenticationHeaderValue = null;
        }

        [When("the current user tries adding a new todo item")]
        public async Task WhenTheCurrentUserTriesAddingANewTodoItem(Table newTodoItemDetailsTable)
        {
            newTodoItemInfo = newTodoItemDetailsTable.CreateInstance<NewTodoItemInfo>
            (
                creationOptions: new InstanceCreationOptions
                {
                    VerifyAllColumnsBound = true
                }
            );

            httpResponseMessage = await todoWebApiDriver.AddNewTodoItemAsync(newTodoItemInfo, authenticationHeaderValue);
        }

        [Then("the system must add the new todo item")]
        public void ThenTheSystemMustAddTheNewTodoItem()
        {
            using AssertionScope _ = new();
            httpResponseMessage.Should().NotBeNull();
            httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
        }

        [Then("reply with a success response")]
        public void ThenReplyWithASuccessResponse(Table responseDetailsTable)
        {
            string httpStatusCodeAsString = responseDetailsTable.Rows[0]["HttpStatusCode"];

            if (Enum.TryParse(httpStatusCodeAsString, out HttpStatusCode expectedStatusCode) is false)
            {
                throw new ArgumentException
                (
                    message: $"Cannot parse {nameof(HttpStatusCode)} from value: \"{httpStatusCodeAsString}\"",
                    paramName: nameof(responseDetailsTable)
                );
            }

            using AssertionScope _ = new();
            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
            httpResponseMessage.Headers.Location.Should().NotBeNull();
            httpResponseMessage.Headers.Location!.IsAbsoluteUri.Should().BeTrue();
            httpResponseMessage.Headers.Location!.ToString().Should().Match(responseDetailsTable.Rows[0]["LocationHeaderValueMatchExpression"]);
        }

        [Then("the system must not add the new todo item")]
        public void ThenTheSystemMustNotAddTheNewTodoItem()
        {
            using AssertionScope _ = new();
            httpResponseMessage.Should().NotBeNull();
            httpResponseMessage.IsSuccessStatusCode.Should().BeFalse();
        }

        [Then("reply with a failed response")]
        public void ThenReplyWithAFailedResponse(Table responseDetailsTable)
        {
            string httpStatusCodeAsString = responseDetailsTable.Rows[0]["HttpStatusCode"];

            if (Enum.TryParse(httpStatusCodeAsString, out HttpStatusCode expectedStatusCode) is false)
            {
                throw new ArgumentException
                (
                    message: $"Cannot parse {nameof(HttpStatusCode)} from value: \"{httpStatusCodeAsString}\"",
                    paramName: nameof(responseDetailsTable)
                );
            }

            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
        }
    }
}
