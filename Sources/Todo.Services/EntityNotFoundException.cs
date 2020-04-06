using System;
using System.Runtime.Serialization;

namespace Todo.Services
{
    /// <summary>
    /// Thrown when failed to fetch an entity using a key.
    /// </summary>
    [Serializable]
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(Type entityType, object entityKey) : base(
            $"Could not find entity of type \"{entityType?.FullName}\" using key \"{entityKey}\"")
        {
        }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        protected EntityNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {
        }

        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }
    }
}