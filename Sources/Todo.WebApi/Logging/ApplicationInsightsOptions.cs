﻿namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Stores Azure Application Insights configuration.
    /// </summary>
    public class ApplicationInsightsOptions
    {
        /// <summary>
        /// Gets or sets the instrumentation key pointing to the proper Azure Application Insights instance.
        /// </summary>
        public string InstrumentationKey {get; set;}
    }
}
