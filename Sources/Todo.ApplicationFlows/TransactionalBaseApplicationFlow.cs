using System;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;

namespace Todo.ApplicationFlows
{
    /// <summary>
    /// Base class for all application flows which make use of transactions.
    /// </summary>
    public abstract class TransactionalBaseApplicationFlow<TInput, TOutput> : NonTransactionalBaseApplicationFlow<TInput, TOutput>
    {
        /// <summary>
        /// Creates a new instance of a particular application flow.
        /// </summary>
        /// <param name="flowName">The name used for identifying the flow.</param>
        /// <param name="logger">The <see cref="ILogger"/> instance used for logging any message originating
        /// from the flow.</param>
        /// <exception cref="ArgumentException">Thrown in case the given <paramref name="flowName"/> is null or
        /// white-space only.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the given <paramref name="logger"/> is null</exception>
        protected TransactionalBaseApplicationFlow(string flowName, ILogger logger): base(flowName, logger)
        {
        }
       
        /// <summary>
        /// Performs operations common to any application flow: validating the input,
        /// wrapping the flow in a transaction and finally executing each flow step.
        /// </summary>
        /// <param name="input">The flow input.</param>
        /// <param name="flowInitiator">The user who initiated executing this flow.</param>
        /// <returns>The flow output.</returns>
        protected override async Task<TOutput> InternalExecuteAsync(TInput input, IPrincipal flowInitiator)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(value: 60)
            };

            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);
            TOutput output = await base.InternalExecuteAsync(input, flowInitiator).ConfigureAwait(false);
            transactionScope.Complete();
            return output;
        }
    }
}