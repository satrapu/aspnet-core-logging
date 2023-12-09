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
        public static async Task ExecuteAsync(string flowName, Func<Task> action, IPrincipal flowInitiator, ILogger logger)
        {
            InternalNonTransactionalApplicationFlow applicationFlow = new(flowName, action, logger);
            await applicationFlow.ExecuteAsync(null, flowInitiator);
        }

        private sealed class InternalNonTransactionalApplicationFlow : NonTransactionalBaseApplicationFlow<object, object>
        {
            private readonly Func<Task> asyncFlow;

            public InternalNonTransactionalApplicationFlow(string flowName, Func<Task> asyncFlow, ILogger logger) : base(flowName, logger)
            {
                this.asyncFlow = asyncFlow;
            }

            protected override async Task<object> ExecuteFlowStepsAsync(object input, IPrincipal flowInitiator)
            {
                await asyncFlow();
                return Task.FromResult((object)null);
            }
        }
    }
}
