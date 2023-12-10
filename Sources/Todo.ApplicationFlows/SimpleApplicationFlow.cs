namespace Todo.ApplicationFlows
{
    using System;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Executes <see cref="Action"/> instances using the same mechanism employed when executing heavier
    /// application flows which implement <see cref="IApplicationFlow{TInput, TOutput}"/> interface.
    /// </summary>
    public static class SimpleApplicationFlow
    {
        public static async Task ExecuteAsync(string flowName, Func<Task> simpleApplicationFlow, IPrincipal flowInitiator, ILogger logger)
        {
            await new InternalNonTransactionalApplicationFlow(flowName, simpleApplicationFlow, logger).ExecuteAsync(null, flowInitiator);
        }

        private sealed class InternalNonTransactionalApplicationFlow : NonTransactionalBaseApplicationFlow<object, object>
        {
            private readonly Func<Task> simpleApplicationFlow;

            public InternalNonTransactionalApplicationFlow(string flowName, Func<Task> simpleApplicationFlow, ILogger logger) : base(flowName, logger)
            {
                this.simpleApplicationFlow = simpleApplicationFlow;
            }

            protected override async Task<object> ExecuteFlowStepsAsync(object input, IPrincipal flowInitiator)
            {
                await simpleApplicationFlow();
                return Task.CompletedTask;
            }
        }
    }
}
