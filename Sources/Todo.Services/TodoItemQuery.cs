using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Todo.Services
{
    public class TodoItemQuery
    {
        public long? Id { get; set; }

        public string NamePattern { get; set; }

        public bool? IsComplete { get; set; }

        [Required]
        public ClaimsPrincipal User { get; set; }
    }
}
