namespace Todo.ApplicationFlows
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Todo.Services.Security;

    /// <summary>
    /// Base class for all application flows which do not need transactions.
    /// </summary>
    public abstract class NonTransactionalBaseApplicationFlow<TInput, TOutput> : IApplicationFlow<TInput, TOutput>
    {
        private const string ApplicationFlowName = "ApplicationFlowName";
        private readonly string flowName;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of a particular application flow.
        /// </summary>
        /// <param name="flowName">The name used for identifying the flow.</param>
        /// <param name="logger">The <see cref="ILogger"/> instance used for logging any message originating
        /// from the flow.</param>
        /// <exception cref="ArgumentException">Thrown in case the given <paramref name="flowName"/> is null or
        /// white-space only.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the given <paramref name="logger"/> is null</exception>
        protected NonTransactionalBaseApplicationFlow(string flowName, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(flowName))
            {
                throw new ArgumentException("Flow name cannot be null or whitespace", nameof(flowName));
            }

            this.flowName = flowName;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// See more here: <see cref="IApplicationFlow{TInput,TOutput}.ExecuteAsync"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="flowInitiator"></param>
        /// <returns></returns>
        public async Task<TOutput> ExecuteAsync(TInput input, IPrincipal flowInitiator)
        {
            using (logger.BeginScope(new Dictionary<string, object> { [ApplicationFlowName] = flowName }))
            {
                bool isSuccess = false;
                Stopwatch stopwatch = Stopwatch.StartNew();
                string flowInitiatorName = flowInitiator.GetNameOrDefault();

                try
                {
                    logger.LogInformation(
                        "User [{FlowInitiator}] has started executing application flow [{ApplicationFlowName}] ...",
                        flowInitiatorName, flowName);
                    TOutput output = await InternalExecuteAsync(input, flowInitiator);
                    isSuccess = true;
                    return output;
                }
                finally
                {
                    stopwatch.Stop();
                    logger.LogInformation(
                        "User [{FlowInitiator}] has finished executing application flow [{ApplicationFlowName}] "
                        + "with the outcome: [{ApplicationFlowOutcome}]; "
                        + "time taken: [{ApplicationFlowDurationAsTimeSpan}] ({ApplicationFlowDurationInMillis}ms)",
                        flowInitiatorName, flowName, isSuccess ? "success" : "failure", stopwatch.Elapsed,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>
        /// Performs operations common to any application flow: validating the input,
        /// wrapping the flow in a transaction and finally executing each flow step.
        /// </summary>
        /// <param name="input">The flow input.</param>
        /// <param name="flowInitiator">The user who initiated executing this flow.</param>
        /// <returns>The flow output.</returns>
        protected virtual async Task<TOutput> InternalExecuteAsync(TInput input, IPrincipal flowInitiator)
        {
            TOutput output = await ExecuteFlowStepsAsync(input, flowInitiator);
            return output;
        }

        /// <summary>
        /// Executes the steps of this particular flow.
        /// </summary>
        /// <param name="input">The flow input.</param>
        /// <param name="flowInitiator">The user who initiated executing this flow.</param>
        /// <returns>The flow output.</returns>
        protected abstract Task<TOutput> ExecuteFlowStepsAsync(TInput input, IPrincipal flowInitiator);
    }
}
