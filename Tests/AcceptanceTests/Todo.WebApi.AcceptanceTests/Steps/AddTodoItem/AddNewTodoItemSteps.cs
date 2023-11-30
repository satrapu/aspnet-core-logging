namespace Todo.WebApi.AcceptanceTests.Steps.AddTodoItem
{
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
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef
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

        [When("the current user adds a new todo item using the below details")]
        public void WhenTheCurrentUserAddsANewTodoItemUsingTheBelowDetails(Table newTodoItemDetailsTable)
        {
            newTodoItemInfo = newTodoItemDetailsTable.CreateInstance<NewTodoItemInfo>
            (
                creationOptions: new InstanceCreationOptions { VerifyAllColumnsBound = true }
            );
        }

        [Then("the system must create the todo item")]
        public async Task ThenTheSystemMustCreateTheTodoItem()
        {
            HttpResponseMessage httpResponseMessage =
                await todoWebApiDriver.AddNewTodoItemAsync(newTodoItemInfo, authenticationHeaderValue);

            using AssertionScope _ = new();
            httpResponseMessage.Should().NotBeNull();
            httpResponseMessage.Should().Be201Created();
            httpResponseMessage.Should().HaveHeader("Location");
            httpResponseMessage.Headers.Location?.IsAbsoluteUri.Should().BeTrue();
            httpResponseMessage.Headers.Location?.ToString().Should().Match("http*:*//*/api/todo/*");
        }
    }
}
