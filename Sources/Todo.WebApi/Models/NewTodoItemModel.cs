using System.ComponentModel.DataAnnotations;

namespace Todo.WebApi.Models
{
    public class NewTodoItemModel
    {
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
        public bool? IsComplete { get; set; }
    }
}
