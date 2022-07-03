namespace Todo.Commons
{
    using System.Diagnostics;
    using System.Reflection;

    public static class ActivitySources
    {
        private const string ActivitySourceName = "Todo";

        static ActivitySources()
        {
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
