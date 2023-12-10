namespace Todo.Commons.Diagnostics
{
    using System.Diagnostics;
    using System.Reflection;

    public static class ActivitySources
    {
        private const string ActivitySourceName = "TodoWebApi";

        static ActivitySources()
        {
            TodoWebApi = new ActivitySource
            (
                name: ActivitySourceName,
                version: Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion
            );
        }

        public static ActivitySource TodoWebApi { get; }
    }
}
