using System;

namespace Todo.Services
{
    /// <summary>
    /// Thrown when failed to fetch an entity using a key.
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(Type entityType, object entityKey) : base(
            $"Could not find entity of type \"{entityType?.FullName}\" using key \"{entityKey}\"")
        {
        }
    }
}