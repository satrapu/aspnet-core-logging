namespace Todo.WebApi.Models
{
    /// <summary>
    /// Contains options used when generating JSON web tokens.
    /// </summary>
    public class GenerateJwtOptions
    {
        /// <summary>
        /// Gets or sets the audience for the JSON web tokens.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public string Issuer { get; set; }

        public string Secret { get; set; }
    }
}
