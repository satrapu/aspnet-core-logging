namespace Todo.WebApi.Models
{
    using System;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Populates <see cref="GenerateJwtOptions"/> instances with values coming from application configuration.
    /// </summary>
    public class GenerateJwtOptionsSetup : IConfigureOptions<GenerateJwtOptions>
    {
        private const string SectionName = "GenerateJwt";
        private readonly IConfiguration configuration;

        public GenerateJwtOptionsSetup(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Configure(GenerateJwtOptions options)
        {
            configuration.GetSection(SectionName).Bind(options);
        }
    }
}
