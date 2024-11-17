using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using VerifyTests;

namespace Todo.WebApi
{
    public static partial class ModuleInitializer
    {
        private static readonly Regex RegexForItemUrl = GeneratedRegexForItemUrl();

        internal static readonly VerifySettings VerifySettings = new();

        [ModuleInitializer]
        public static void Initialize()
        {
            VerifierSettings.InitializePlugins();
            Recording.Start();

            VerifySettings.UseDirectory("VerifySnapshots");
            VerifySettings.ScrubEmptyLines();
            VerifySettings.ScrubInlineGuids();
            VerifySettings.ScrubMember("TraceId");
            VerifySettings.ScrubMember("traceId");
            VerifySettings.ScrubLinesWithReplace
            (
                line => RegexForItemUrl.IsMatch(line)
                    ? "http://localhost/api/todo/SomeTodoItemId"
                    : line
            );
        }

        [GeneratedRegex(@"^http://localhost/api/todo/\d+$", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex GeneratedRegexForItemUrl();
    }
}
