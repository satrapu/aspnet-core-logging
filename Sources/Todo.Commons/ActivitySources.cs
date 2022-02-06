namespace Todo.Commons
{
    using System.Diagnostics;
    using System.Reflection;

    public static class ActivitySources
    {
        private const string ActivitySourceName = "Todo";

        static ActivitySources()
        {
            var activityListener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions)
                    => ActivitySamplingResult.AllDataAndRecorded,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions)
                    => ActivitySamplingResult.AllDataAndRecorded,
            };

            ActivitySource.AddActivityListener(activityListener);

            string applicationInformationalVersion =
                Assembly
                    .GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;

            TodoActivitySource = new ActivitySource(name: ActivitySourceName, version: applicationInformationalVersion);
        }

        public static ActivitySource TodoActivitySource { get; }
    }
}
