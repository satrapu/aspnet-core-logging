using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;

namespace Todo.Services
{
    /// <summary>
    /// Base class for all application flows.
    /// </summary>
    public abstract class BaseApplicationFlow<TInput, TOutput> : IApplicationFlow<TInput, TOutput>
    {
        private readonly string flowName;
        private readonly ILogger logger;

        protected BaseApplicationFlow(string flowName, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(flowName))
            {
                throw new ArgumentException(nameof(flowName));
            }

            this.flowName = flowName;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TOutput> ExecuteAsync(TInput input, IPrincipal flowInitiator)
        {
            using (logger.BeginScope(new Dictionary<string, object> {["BusinessFlowName"] = flowName}))
            {
                bool isSuccess = false;
                Stopwatch stopwatch = Stopwatch.StartNew();
                string flowInitiatorName = flowInitiator.GetNameOrDefault();

                try
                {
                    logger.LogInformation("User [{User}] has started application flow [{ApplicationFlow}] ...",
                        flowInitiatorName, flowName);
                    TOutput output = await ExecuteFlowAsync(input).ConfigureAwait(false);
                    isSuccess = true;
                    return output;
                }
                finally
                {
                    stopwatch.Stop();
                    logger.LogInformation("User [{User}] has finished application flow [{ApplicationFlow}] "
                                          + "with the outcome: [{ApplicationFlowOutcome}]; time taken: [{ApplicationFlowDuration}]",
                        flowInitiatorName, flowName, isSuccess ? "success" : "failure",
                        stopwatch.Elapsed);
                }
            }
        }

        private async Task<TOutput> ExecuteFlowAsync(TInput input)
        {
            Validator.ValidateObject(input, new ValidationContext(input), validateAllProperties: true);

            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(value: 60)
            };

            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);
            TOutput output = await ExecuteFlowStepsAsync(input).ConfigureAwait(false);
            transactionScope.Complete();
            return output;
        }

        protected abstract Task<TOutput> ExecuteFlowStepsAsync(TInput input);
    }
}