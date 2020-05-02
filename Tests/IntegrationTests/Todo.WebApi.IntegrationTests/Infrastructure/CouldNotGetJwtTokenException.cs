using System;
using System.Net.Http;

namespace Todo.WebApi.Infrastructure
{
    public class CouldNotGetJwtException : Exception
    {
        public CouldNotGetJwtException(HttpResponseMessage httpResponseMessage) : base(
            $"Failed to get an JSON web token for testing purposes due to: {httpResponseMessage.ReasonPhrase}")
        {
        }
    }
}