using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using VerifyTests;

namespace Todo.WebApi
{
    public static partial class ModuleInitializer
    {
        private static readonly Regex RegexForItemUrl = GeneratedRegexForItemUrl();
        private static readonly Regex RegexForTraceIdHeaderValue = GeneratedRegexForTraceIdHeaderValue();
        internal static readonly VerifySettings VerifySettingsForIntegrationsTests = new();

        [ModuleInitializer]
        public static void Initialize()
        {
            VerifierSettings.InitializePlugins();
            Recording.Start();

            VerifySettingsForIntegrationsTests.UseDirectory("VerifySnapshots");
            VerifySettingsForIntegrationsTests.IgnoreMember("TraceId");
            VerifySettingsForIntegrationsTests.ScrubEmptyLines();
            VerifySettingsForIntegrationsTests.ScrubInlineGuids();
            VerifySettingsForIntegrationsTests.ScrubLinesWithReplace
            (
                line => RegexForItemUrl.IsMatch(line)
                    ? "http://localhost/api/todo/SomeTodoItemId"
                    : line
            );
            VerifySettingsForIntegrationsTests.ScrubLinesWithReplace
            (
                line => RegexForTraceIdHeaderValue.IsMatch(line)
                    ? "SomeTraceId"
                    : line
            );
        }

        [GeneratedRegex(@"^http://localhost/api/todo/\d+$", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex GeneratedRegexForItemUrl();

        [GeneratedRegex(@"^0H\w+$", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex GeneratedRegexForTraceIdHeaderValue();
    }
}
