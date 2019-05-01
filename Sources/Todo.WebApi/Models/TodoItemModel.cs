namespace Todo.WebApi.Models
{
    public class TodoItemModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
