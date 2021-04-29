namespace Todo.WebApi.Models
{
    using System.Diagnostics.CodeAnalysis;

    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class GenerateJwtOptions
    {
        public string Audience { get; set; }

        public string Issuer { get; set; }

        public string Secret { get; set; }
    }
}
