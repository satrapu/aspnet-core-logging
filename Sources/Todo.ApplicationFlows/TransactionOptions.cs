using System;

namespace Todo.ApplicationFlows
{
    public class TransactionOptions
    {
        public System.Transactions.IsolationLevel IsolationLevel { get; set; }

        public TimeSpan Timeout { get; set; }
    }
}
