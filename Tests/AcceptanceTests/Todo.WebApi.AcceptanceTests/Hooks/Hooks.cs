namespace Todo.WebApi.AcceptanceTests.Hooks
{
    using System.Threading.Tasks;
    using System.Diagnostics.CodeAnalysis;

    using Dependencies;

    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Infrastructure;

    [Binding]
    [SuppressMessage("Sonar", "S1118", Justification = "Class must be instantiable by SpecFlow test runner")]
    public class Hooks
    {
        private const string SystemUnderTestProcessKey = "Todo.WebApi.Process";
        private const string SystemUnderTestPortKey = "Todo.WebApi.Port";

        [BeforeFeature]
        public static async Task StartApplication(FeatureContext featureContext)
        {
            TcpPortProvider tcpPortProvider = featureContext.FeatureContainer.Resolve<TcpPortProvider>();
            int port = tcpPortProvider.GetAvailableTcpPort();

            ISpecFlowOutputHelper specFlowOutputHelper = featureContext.FeatureContainer.Resolve<ISpecFlowOutputHelper>();
            SystemUnderTest systemUnderTestProcess = await SystemUnderTest.StartNewAsync(port, specFlowOutputHelper);

            featureContext.Add(SystemUnderTestProcessKey, systemUnderTestProcess);
            featureContext.Add(SystemUnderTestPortKey, port);
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
