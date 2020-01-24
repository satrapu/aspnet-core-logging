using System;

namespace Todo.Persistence.Entities
{
    /// <summary>
    /// Represents an action to be performed at some point in the future.
    /// </summary>
    public class TodoItem
    {
        /// <summary>
        /// Gets or sets the identifier of this action.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Gets or sets the name of this action.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether this action has been completed.
        /// </summary>
        public bool IsComplete { get; set; }

        public string CreatedBy { get; }

        public DateTime CreatedOn { get; }

        public string LastUpdatedBy { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public TodoItem(string name, string createdBy)
        {
            Name = name;
            CreatedBy = createdBy;
            CreatedOn = DateTime.UtcNow;
            LastUpdatedBy = CreatedBy;
            LastUpdatedOn = CreatedOn;
            IsComplete = false;
        }

        /// <summary>
        /// Gets the string representation of this action.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{nameof(Id)}: {Id}, " +
                   $"{nameof(Name)}: {Name}, " +
                   $"{nameof(IsComplete)}: {IsComplete}, " +
                   $"{nameof(CreatedBy)}: {CreatedBy}" +
                   $"{nameof(CreatedOn)}: {CreatedOn:u}" +
                   $"{nameof(LastUpdatedBy)}: {LastUpdatedBy}" +
                   $"{nameof(LastUpdatedOn)}: {LastUpdatedOn:u}]";
        }
    }
}