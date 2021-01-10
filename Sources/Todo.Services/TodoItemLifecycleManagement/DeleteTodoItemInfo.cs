using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace Todo.Services.TodoItemLifecycleManagement
{
    public class DeleteTodoItemInfo
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long? Id { get; set; }

        [Required]
        public IPrincipal Owner { get; set; }
    }
}
