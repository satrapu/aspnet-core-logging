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
        public static void Execute(string flowName, Action action, IPrincipal flowInitiator, ILogger logger)
        {
            var applicationFlow = new InternalNonTransactionalApplicationFlow(flowName, action, logger);
            applicationFlow.ExecuteAsync(null, flowInitiator).Wait();
        }

        private sealed class InternalNonTransactionalApplicationFlow
            : NonTransactionalBaseApplicationFlow<object, object>
        {
            private readonly Action action;

            public InternalNonTransactionalApplicationFlow(string flowName, Action action, ILogger logger)
                : base(flowName, logger)
            {
                this.action = action;
            }

            protected override Task<object> ExecuteFlowStepsAsync(object input, IPrincipal flowInitiator)
            {
                action();
                return Task.FromResult((object) null);
            }
        }
    }
}
