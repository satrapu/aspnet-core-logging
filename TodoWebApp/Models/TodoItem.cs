using System.ComponentModel.DataAnnotations;

namespace TodoWebApp.Models
{
    public class TodoItem
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public bool IsComplete { get; set; }
    }
}