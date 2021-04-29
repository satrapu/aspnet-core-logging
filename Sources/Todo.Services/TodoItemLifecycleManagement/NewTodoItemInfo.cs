namespace Todo.Services.TodoItemLifecycleManagement
{
    using System.ComponentModel.DataAnnotations;
    using System.Security.Principal;

    public class NewTodoItemInfo
    {
        [Required(AllowEmptyStrings = false)]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public bool? IsComplete { get; set; }

        [Required]
        public IPrincipal Owner { get; set; }
    }
}
