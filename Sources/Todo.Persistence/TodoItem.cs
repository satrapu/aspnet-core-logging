using System;
using System.ComponentModel.DataAnnotations;

namespace Todo.Persistence
{
    /// <summary>
    /// Represents an action to be performed at some point in the future.
    /// </summary>
    public class TodoItem
    {
        /// <summary>
        /// Gets or sets the identifier of this action.
        /// </summary>
        [Required]
        [Range(1, long.MaxValue)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of this action.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether this action has been completed.
        /// </summary>
        [Required]
        public bool IsComplete { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [MinLength(2)]
        [MaxLength(100)]
        public string LastUpdatedBy { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        /// <summary>
        /// Gets the string representation of this action.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(IsComplete)}: {IsComplete}]";
        }
    }
}