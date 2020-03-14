using System;
using System.Net.Http;

namespace Todo.WebApi.Infrastructure
{
    public class CouldNotGetTokenException : Exception
    {
        public CouldNotGetTokenException(HttpResponseMessage httpResponseMessage) : base(
            $"Failed to get an access token for testing purposes due to: {httpResponseMessage.ReasonPhrase}")
        {
        }
    }
}