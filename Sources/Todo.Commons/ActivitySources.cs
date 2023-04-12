namespace Todo.Commons
{
    using System.Diagnostics;
    using System.Reflection;

    public static class ActivitySources
    {
        private const string ActivitySourceName = "TodoWebApi";

        static ActivitySources()
        {
            string applicationInformationalVersion =
                Assembly
                    .GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;

            TodoWebApi = new(name: ActivitySourceName, version: applicationInformationalVersion);
        }

        public static ActivitySource TodoWebApi { get; }
    }
}
