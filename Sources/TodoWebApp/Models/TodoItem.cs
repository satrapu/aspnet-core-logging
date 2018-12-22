using System.ComponentModel.DataAnnotations;

namespace TodoWebApp.Models
{
    /// <summary>
    /// Represents a to do item.
    /// </summary>
    public class TodoItem
    {
        /// <summary>
        /// Gets or sets the identifier of this to do item.
        /// </summary>
        [Required]
        [Range(1, long.MaxValue)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of this to do item.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether this to do item has been completed.
        /// </summary>
        [Required]
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets the string representation of this to do item.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(IsComplete)}: {IsComplete}]";
        }
    }
}