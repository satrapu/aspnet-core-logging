namespace Todo.WebApi.Models
{
    public class TodoItemQueryModel
    {
        public long? Id { get; set; }

        public string NamePattern { get; set; }

        public bool? IsComplete { get; set; }
    }
}
