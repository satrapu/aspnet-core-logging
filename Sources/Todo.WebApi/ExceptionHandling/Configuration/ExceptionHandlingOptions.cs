namespace Todo.WebApi.ExceptionHandling.Configuration
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Contains exception handling related configuration options.
    /// </summary>
    public class ExceptionHandlingOptions
    {
        /// <summary>
        /// Gets or sets whether to include details when converting an instance of <see cref="System.Exception"/> class
        /// to an instance of <see cref="ProblemDetails"/> class.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IncludeDetails { get; set; }
    }
}
