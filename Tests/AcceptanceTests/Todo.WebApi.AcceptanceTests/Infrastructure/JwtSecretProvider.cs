namespace Todo.WebApi.AcceptanceTests.Infrastructure
{
    using System;

    public class JwtSecretProvider
    {
        private static readonly string Secret;

        static JwtSecretProvider()
        {
            Secret = Guid.NewGuid().ToString("N");
        }

        public string GetSecret() => Secret;
    }
}
