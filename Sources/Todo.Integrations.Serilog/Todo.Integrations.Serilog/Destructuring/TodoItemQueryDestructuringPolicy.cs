namespace Todo.Integrations.Serilog.Destructuring
{
    using System.Collections.Generic;

    using global::Serilog.Core;
    using global::Serilog.Events;

    using Services.Security;
    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Instructs Serilog how to log instances of <seealso cref="TodoItemQuery"/> class.
    /// </summary>
    public class TodoItemQueryDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
            out LogEventPropertyValue result)
        {
            result = null;
            TodoItemQuery todoItemQuery = value as TodoItemQuery;

            if (todoItemQuery == null)
            {
                return false;
            }

            result = new StructureValue(new List<LogEventProperty>
            {
                new LogEventProperty(nameof(todoItemQuery.Id), new ScalarValue(todoItemQuery.Id)),
                new LogEventProperty(nameof(todoItemQuery.NamePattern), new ScalarValue(todoItemQuery.NamePattern)),
                new LogEventProperty(nameof(todoItemQuery.IsComplete), new ScalarValue(todoItemQuery.IsComplete)),
                new LogEventProperty(nameof(todoItemQuery.Owner),
                    new ScalarValue(todoItemQuery.Owner.GetNameOrDefault())),
                new LogEventProperty(nameof(todoItemQuery.PageIndex), new ScalarValue(todoItemQuery.PageIndex)),
                new LogEventProperty(nameof(todoItemQuery.PageSize), new ScalarValue(todoItemQuery.PageSize)),
                new LogEventProperty(nameof(todoItemQuery.SortBy), new ScalarValue(todoItemQuery.SortBy)),
                new LogEventProperty(nameof(todoItemQuery.IsSortAscending),
                    new ScalarValue(todoItemQuery.IsSortAscending))
            });

            return true;
        }
    }
}
