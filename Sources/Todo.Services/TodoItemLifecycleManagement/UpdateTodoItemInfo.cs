namespace Todo.Services.TodoItemLifecycleManagement
{
    using System.ComponentModel.DataAnnotations;
    using System.Security.Principal;

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
        public IPrincipal Owner { get; set; }
    }
}
