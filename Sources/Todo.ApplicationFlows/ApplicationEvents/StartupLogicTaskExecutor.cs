namespace Todo.ApplicationFlows.ApplicationEvents
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Commons.StartupLogic;

    /// <summary>
    /// An <see cref="IStartupLogicTaskExecutor"/> implementation.
    /// </summary>
    public class StartupLogicTaskExecutor : IStartupLogicTaskExecutor
    {
        private const string FlowName = "ApplicationStartup/ExecuteStartupLogicTasks";
        private static readonly IPrincipal Principal = new GenericPrincipal(new GenericIdentity("execute-application-startup-logic"), Array.Empty<string>());

        private readonly IEnumerable<IStartupLogicTask> startupLogicTasks;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILogger logger;

        public StartupLogicTaskExecutor
        (
            IEnumerable<IStartupLogicTask> startupLogicTasks,
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<StartupLogicTaskExecutor> logger
        )
        {
            this.startupLogicTasks = startupLogicTasks ?? throw new ArgumentNullException(nameof(startupLogicTasks));
            this.hostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task ExecuteAsync()
        {
            return SimpleApplicationFlow.ExecuteAsync(FlowName, InternalExecuteAsync, Principal, logger);
        }

        private async Task InternalExecuteAsync()
        {
            try
            {
                foreach (IStartupLogicTask startupLogicTask in startupLogicTasks)
                {
                    string startupLogicTaskName = startupLogicTask.GetType().AssemblyQualifiedName;

                    logger.LogInformation("Application startup logic task: [{ApplicationStartedEventListener}] is about to run ...", startupLogicTaskName);
                    await startupLogicTask.ExecuteAsync();
                    logger.LogInformation("Application startup logic task: [{ApplicationStartedEventListener}] has run successfully", startupLogicTaskName);
                }
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error has occurred while executing application startup logic; application will stop");
                hostApplicationLifetime.StopApplication();
            }
        }
    }
}
