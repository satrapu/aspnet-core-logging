namespace Todo.WebApi.AcceptanceTests.Steps
{
    using TechTalk.SpecFlow;

    [Binding]
    public class AddNewTodoItemSteps
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef
        private readonly ScenarioContext scenarioContext;

        public AddNewTodoItemSteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [Given(@"the current user is authorized to add a new todo item")]
        public void GivenTheCurrentUserIsAuthorizedToAddANewTodoItem()
        {
            scenarioContext.Pending();
        }

        [When(@"the current user adds a new todo item using the below details")]
        public void WhenTheCurrentUserAddsANewTodoItemUsingTheBelowDetails(Table table)
        {
            scenarioContext.Pending();
        }

        [Then(@"the system must store that todo item")]
        public void ThenTheSystemMustStoreThatTodoItem()
        {
            scenarioContext.Pending();
        }
    }
}
