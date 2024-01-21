namespace Todo.WebApi.AcceptanceTests.Hooks
{
    using System.Threading.Tasks;
    using System.Diagnostics.CodeAnalysis;

    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Infrastructure;

    using Infrastructure;

    [Binding]
    [SuppressMessage("Sonar", "S1118", Justification = "Class must be instantiable by SpecFlow test runner")]
    public class Hooks
    {
        private const string SystemUnderTestProcessKey = $"{nameof(SystemUnderTest)}.Process";

        [BeforeFeature]
        public static async Task StartApplication(FeatureContext featureContext)
        {
            SystemUnderTest systemUnderTestProcess = await SystemUnderTest.StartNewAsync
            (
                port: featureContext.FeatureContainer.Resolve<TcpPortProvider>().GetAvailableTcpPort(),
                specFlowOutputHelper: featureContext.FeatureContainer.Resolve<ISpecFlowOutputHelper>()
            );

            featureContext.Add(SystemUnderTestProcessKey, systemUnderTestProcess);
        }

        [AfterFeature]
        public static async Task StopApplication(FeatureContext featureContext)
        {
            SystemUnderTest systemUnderTest = featureContext.Get<SystemUnderTest>(SystemUnderTestProcessKey);

            if (systemUnderTest is not null)
            {
                await systemUnderTest.DisposeAsync();
            }
        }
    }
}
