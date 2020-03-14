using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;

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
        public int PageIndex { get; set; }

        public string SortBy { get; set; }

        public bool? IsSortAscending { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendJoin(";",
                $"[{nameof(Id)}={Id}",
                $"{nameof(NamePattern)}={NamePattern}",
                $"{nameof(IsComplete)}={IsComplete}",
                $"{nameof(User)}={User.GetUserId()}",
                $"{nameof(PageIndex)}={PageIndex}",
                $"{nameof(PageSize)}={PageSize}",
                $"{nameof(SortBy)}={SortBy}",
                $"{nameof(IsSortAscending)}={IsSortAscending}]");
            return stringBuilder.ToString();
        }
    }
}