namespace Todo.ApplicationFlows
{
    using System;
    using System.Transactions;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TransactionOptions
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IsolationLevel IsolationLevel { get; set; }

        public TimeSpan Timeout { get; set; }
    }
}
