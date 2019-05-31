using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Todo.TestInfrastructure.Logging
{
    public static class LoggerExtensions
    {
        public static void LogMethodEntered(this ILogger logger, [CallerMemberName] string memberName = "")
        {
            logger.LogInformation("{MemberName} - BEGIN", memberName);
        }

        public static void LogMethodExited(this ILogger logger, [CallerMemberName] string memberName = "")
        {
            logger.LogInformation("{MemberName} - END", memberName);
        }
    }
}
