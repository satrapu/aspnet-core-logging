namespace Todo.WebApi.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Contains options used when generating JSON web tokens.
    /// </summary>
    public class GenerateJwtOptions
    {
        /// <summary>
        /// Gets or sets the audience for the JSON web tokens.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Audience { get; init; }

        /// <summary>
        /// Gets or sets the issuer of the JSON web tokens.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Issuer { get; init; }

        /// <summary>
        /// Gets or sets the secret needed when creating JSON web tokens.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Secret { get; init; }
    }
}
