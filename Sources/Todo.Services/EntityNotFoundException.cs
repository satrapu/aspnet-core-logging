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
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EntityNotFoundException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }
}