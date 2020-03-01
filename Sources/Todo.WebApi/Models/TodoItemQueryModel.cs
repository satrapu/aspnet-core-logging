using System.ComponentModel.DataAnnotations;

namespace Todo.WebApi.Models
{
    public class TodoItemQueryModel
    {
        public long? Id { get; set; }

        public string NamePattern { get; set; }

        public bool? IsComplete { get; set; }

        [Range(1, 1000)] 
        public int PageSize { get; set; } = 25;

        [Range(0, int.MaxValue)] 
        public int PageIndex { get; set; } = 0;
    }
}