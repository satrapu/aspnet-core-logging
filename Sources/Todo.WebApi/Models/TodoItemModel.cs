// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Todo.WebApi.Models
{
    using System;

    public class TodoItemModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsComplete { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string LastUpdatedBy { get; set; }

        public DateTime? LastUpdatedOn { get; set; }
    }
}
