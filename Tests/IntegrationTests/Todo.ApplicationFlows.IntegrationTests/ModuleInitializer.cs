namespace Todo.ApplicationFlows
{
    using System.Runtime.CompilerServices;

    using VerifyTests;

    public static class ModuleInitializer
    {
        internal static readonly VerifySettings VerifySettings = new();

        [ModuleInitializer]
        public static void Initialize()
        {
            VerifierSettings.InitializePlugins();
            Recording.Start();

            VerifySettings.UseDirectory("VerifySnapshots");
            VerifySettings.ScrubEmptyLines();
            VerifySettings.ScrubInlineGuids();
        }
    }
}
