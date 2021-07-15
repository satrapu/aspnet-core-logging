namespace Todo.ApplicationFlows
{
    using System;
    using System.Transactions;

    /// <summary>
    /// Configures the behavior of the transactions occurring inside a particular application flow.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TransactionOptions
    {
        /// <summary>
        /// Gets or sets the transaction isolation level.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// Gets or sets the transaction timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; }
    }
}
