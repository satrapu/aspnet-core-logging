namespace Todo.Services.Security
{
    /// <summary>
    /// Contains the details needed to generate a JSON web token based on a user name and password.
    /// </summary>
    public sealed class GenerateJwtInfo
    {
        public string UserName { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Password { get; set; }

        public string Audience { get; set; }

        public string Issuer { get; set; }

        public string Secret { get; set; }

        public string[] Scopes { get; set; }
    }
}
