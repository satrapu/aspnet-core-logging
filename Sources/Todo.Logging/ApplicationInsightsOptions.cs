namespace Todo.Logging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Stores Azure Application Insights configuration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ApplicationInsightsOptions
    {
        /// <summary>
        /// Gets or sets the instrumentation key pointing to the proper Azure Application Insights instance.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string InstrumentationKey { get; set; }
    }
}
