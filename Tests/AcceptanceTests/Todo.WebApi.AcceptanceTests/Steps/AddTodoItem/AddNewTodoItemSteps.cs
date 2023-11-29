namespace Todo.WebApi.AcceptanceTests.Steps.AddTodoItem
{
    using System.Net.Http;
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

        public AddNewTodoItemSteps(TodoWebApiDriver todoWebApiDriver)
        {
            this.todoWebApiDriver = todoWebApiDriver;
        }

        [Given(@"the current user is authorized to add a new todo item")]
        public void GivenTheCurrentUserIsAuthorizedToAddANewTodoItem()
        {
            // TODO satrapu 2023-11-29: Create an authorized user
        }

        [When(@"the current user adds a new todo item using the below details")]
        public void WhenTheCurrentUserAddsANewTodoItemUsingTheBelowDetails(Table newTodoItemDetailsTable)
        {
            newTodoItemInfo = newTodoItemDetailsTable.CreateInstance<NewTodoItemInfo>();
        }

        [Then(@"the system must store that todo item")]
        public async Task ThenTheSystemMustStoreThatTodoItem()
        {
            HttpResponseMessage httpResponseMessage = await todoWebApiDriver.AddNewTodoItemAsync(newTodoItemInfo);

            using AssertionScope _ = new();
            httpResponseMessage.Should().NotBeNull();
            httpResponseMessage.Should().Be201Created();
            httpResponseMessage.Should().HaveHeader("Location");
        }
    }
}
