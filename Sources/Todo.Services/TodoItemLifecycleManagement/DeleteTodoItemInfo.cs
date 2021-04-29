namespace Todo.Services.TodoItemLifecycleManagement
{
    using System.ComponentModel.DataAnnotations;
    using System.Security.Principal;

    public class DeleteTodoItemInfo
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long? Id { get; set; }

        [Required]
        public IPrincipal Owner { get; set; }
    }
}
