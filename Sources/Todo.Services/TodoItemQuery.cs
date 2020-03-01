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

        [Range(1, 1000)] 
        public int PageSize { get; set; } = 25;

        [Range(0, int.MaxValue)] 
        public int PageIndex { get; set; } = 0;

        public string SortBy { get; set; }

        public bool? IsSortAscending { get; set; }
    }
}