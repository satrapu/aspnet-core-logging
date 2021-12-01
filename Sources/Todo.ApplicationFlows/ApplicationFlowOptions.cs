namespace Todo.ApplicationFlows
{
    /// <summary>
    /// Configures the behavior of a particular application flow.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ApplicationFlowOptions
    {
        /// <summary>
        /// Gets or sets the options to use when configuring the transaction used by a particular application flow.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public TransactionOptions TransactionOptions { get; set; }
    }
}
