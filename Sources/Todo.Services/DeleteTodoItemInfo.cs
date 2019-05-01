using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Todo.Services
{
    public class DeleteTodoItemInfo
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long? Id { get; set; }

        [Required]
        public ClaimsPrincipal User { get; set; }
    }
}
