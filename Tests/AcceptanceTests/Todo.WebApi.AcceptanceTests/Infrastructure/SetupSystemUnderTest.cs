namespace Todo.WebApi.AcceptanceTests.Infrastructure
{
    using Reqnroll;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Todo.Commons.Constants;

    [Binding]
    [SuppressMessage("Sonar", "S1118", Justification = "Class must be instantiable by SpecFlow test runner")]
    public class SetupSystemUnderTest
    {
        private const string SystemUnderTestProcessKey = $"{nameof(SystemUnderTest)}.Process";

        [BeforeFeature]
        public static async Task StartSystemUnderTestAsync(FeatureContext featureContext)
        {
            SystemUnderTest systemUnderTestProcess = await SystemUnderTest.StartNewAsync
            (
                port: featureContext.FeatureContainer.Resolve<TcpPortProvider>().GetAvailableTcpPort(),
                outputHelper: featureContext.FeatureContainer.Resolve<IReqnrollOutputHelper>(),
                environmentVariables: new Dictionary<string, string>() { ["ASPNETCORE_ENVIRONMENT"] = EnvironmentNames.AcceptanceTests }
            );

            featureContext.Add(SystemUnderTestProcessKey, systemUnderTestProcess);
        }

        [AfterFeature]
        public static async Task StopSystemUnderTestAsync(FeatureContext featureContext)
        {
            SystemUnderTest systemUnderTest = featureContext.Get<SystemUnderTest>(SystemUnderTestProcessKey);

            if (systemUnderTest is not null)
            {
                await systemUnderTest.DisposeAsync();
            }
        }
    }
}
