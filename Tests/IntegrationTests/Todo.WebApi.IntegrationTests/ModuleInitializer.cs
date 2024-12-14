namespace Todo.WebApi
{
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    using VerifyTests;

    public static partial class ModuleInitializer
    {
        private static readonly Regex RegexForBearerToken = GetRegexForBearerToken();
        private static readonly Regex RegexForTodoItemDirectUrl = GetRegexForTodoItemDirectUrl();

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
                line =>
                {
                    if (RegexForBearerToken.IsMatch(line))
                    {
                        return "Bearer <BEARER_TOKEN>";
                    }

                    if (RegexForTodoItemDirectUrl.IsMatch(line))
                    {
                        return "http://localhost/api/todo/<TODO_ITEM_ID>";
                    }

                    return line;
                }
            );
        }

        [GeneratedRegex(@"Bearer [a-z0-9\.]+", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
        private static partial Regex GetRegexForBearerToken();

        [GeneratedRegex(@"http://localhost/api/todo/\d+", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex GetRegexForTodoItemDirectUrl();
    }
}
