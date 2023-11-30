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

        public AddNewTodoItemSteps(TodoWebApiDriver todoWebApiDriver)
        {
            this.todoWebApiDriver = todoWebApiDriver;
        }

        [Given("the current user has the below details")]
        public async Task GivenTheCurrentUserHasTheBelowDetails(Table userDetailsTable)
        {
            UserDetails userDetails = userDetailsTable.CreateInstance<UserDetails>
            (
                creationOptions: new InstanceCreationOptions { VerifyAllColumnsBound = true }
            );

            authenticationHeaderValue = await todoWebApiDriver.GetAuthorizationHeaderAsync(userDetails);
        }

        [Given(@"the current user is not authenticated")]
        public void GivenTheCurrentUserIsNotAuthenticated()
        {
            authenticationHeaderValue = null;
        }

        [Given(@"the current user is not authorized")]
        public void GivenTheCurrentUserIsNotAuthorized()
        {
            authenticationHeaderValue = new AuthenticationHeaderValue
            (
                scheme: TodoWebApiDriver.AuthenticationScheme,
                parameter: Guid.NewGuid().ToString("N")
            );
        }

        [When("the current user adds a new todo item using the below details")]
        public void WhenTheCurrentUserAddsANewTodoItemUsingTheBelowDetails(Table newTodoItemDetailsTable)
        {
            newTodoItemInfo = newTodoItemDetailsTable.CreateInstance<NewTodoItemInfo>
            (
                creationOptions: new InstanceCreationOptions { VerifyAllColumnsBound = true }
            );
        }

        [Then("the system must reply with a success response")]
        public async Task ThenTheSystemMustReplyWithASuccessResponse(Table responseDetailsTable)
        {
            if (Enum.TryParse(responseDetailsTable.Rows[0]["HttpStatusCode"],
                out HttpStatusCode expectedStatusCode) is false)
            {
                throw new ArgumentException
                (
                    message: "Failed to parse HttpStatusCode",
                    paramName: nameof(responseDetailsTable)
                );
            }

            HttpResponseMessage httpResponseMessage =
                await todoWebApiDriver.AddNewTodoItemAsync(newTodoItemInfo, authenticationHeaderValue);

            using AssertionScope _ = new();
            httpResponseMessage.Should().NotBeNull();
            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
            httpResponseMessage.Should().HaveHeader("Location");
            httpResponseMessage.Headers.Location?.IsAbsoluteUri.Should().BeTrue();

            httpResponseMessage.Headers.Location?.ToString().Should()
                .Match(responseDetailsTable.Rows[0]["LocationHeaderValueMatchExpression"]);
        }

        [Then(@"the system must reply with an error response with status code (.*)")]
        public async Task ThenTheSystemMustReplyWithAnErrorResponseWithStatusCode(int httpStatusCode)
        {
            if (Enum.TryParse(httpStatusCode.ToString(), out HttpStatusCode expectedStatusCode) is false)
            {
                throw new ArgumentException
                (
                    message: "Failed to parse HttpStatusCode",
                    paramName: nameof(httpStatusCode)
                );
            }

            HttpResponseMessage httpResponseMessage =
                await todoWebApiDriver.AddNewTodoItemAsync(newTodoItemInfo, authenticationHeaderValue);

            using AssertionScope _ = new();
            httpResponseMessage.Should().NotBeNull();
            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
        }
    }
}
