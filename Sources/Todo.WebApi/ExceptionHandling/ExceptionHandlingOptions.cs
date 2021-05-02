namespace Todo.WebApi.ExceptionHandling
{
    /// <summary>
    /// Contains exception handling related configuration options.
    /// </summary>
    public class ExceptionHandlingOptions
    {
        /// <summary>
        /// Gets or sets whether to include details when converting an instance of <seealso cref="System.Exception"/> class
        /// to an instance of <seealso cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> class.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IncludeDetails { get; set; }
    }
}
