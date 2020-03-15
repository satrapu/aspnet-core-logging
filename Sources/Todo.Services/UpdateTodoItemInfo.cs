using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Todo.Services
{
    public class UpdateTodoItemInfo
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long? Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public bool? IsComplete { get; set; }

        [Required]
        public ClaimsPrincipal User { get; set; }
    }
}
