namespace Todo.WebApi.TestInfrastructure
{
    using System;
    using System.Net.Http;

    public class CouldNotGetJwtException : Exception
    {
        public CouldNotGetJwtException(HttpResponseMessage httpResponseMessage) :
            base($"Failed to get an JSON web token for testing purposes due to: [{httpResponseMessage.ReasonPhrase}]. "
                 + $"Detailed error message is: [{httpResponseMessage.Content.ReadAsStringAsync().Result}]")
        {
        }
    }
}
