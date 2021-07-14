namespace Todo.TestInfrastructure
{
    using System;
    using System.Net.Http;
    using System.Runtime.Serialization;

    [Serializable]
    public class CouldNotGetJwtException : Exception
    {
        public CouldNotGetJwtException(HttpResponseMessage httpResponseMessage) :
            base($"Failed to get an JSON web token for testing purposes due to: [{httpResponseMessage.ReasonPhrase}]. "
                 + $"Detailed error message is: [{httpResponseMessage.Content.ReadAsStringAsync().Result}]")
        {
        }

        protected CouldNotGetJwtException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
