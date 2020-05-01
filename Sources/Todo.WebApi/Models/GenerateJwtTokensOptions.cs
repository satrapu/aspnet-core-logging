namespace Todo.WebApi.Models
{
    public class GenerateJwtTokensOptions
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string Secret { get; set; }
    }
}