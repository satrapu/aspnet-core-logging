namespace Todo.Telemetry.Serilog.Destructuring
{
    using System.Collections.Generic;

    using global::Serilog.Core;
    using global::Serilog.Events;

    using Services.Security;
    using Services.TodoItemManagement;

    /// <summary>
    /// Instructs Serilog how to log instances of <see cref="TodoItemQuery"/> class.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class TodoItemQueryDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
            out LogEventPropertyValue result)
        {
            if (value is TodoItemQuery todoItemQuery)
            {
                result = new StructureValue(new List<LogEventProperty>
                {
                    new(nameof(todoItemQuery.Id), new ScalarValue(todoItemQuery.Id)),
                    new(nameof(todoItemQuery.NamePattern), new ScalarValue(todoItemQuery.NamePattern)),
                    new(nameof(todoItemQuery.IsComplete), new ScalarValue(todoItemQuery.IsComplete)),
                    new(nameof(todoItemQuery.Owner),new ScalarValue(todoItemQuery.Owner.GetNameOrDefault())),
                    new(nameof(todoItemQuery.PageIndex), new ScalarValue(todoItemQuery.PageIndex)),
                    new(nameof(todoItemQuery.PageSize), new ScalarValue(todoItemQuery.PageSize)),
                    new(nameof(todoItemQuery.SortBy), new ScalarValue(todoItemQuery.SortBy)),
                    new(nameof(todoItemQuery.IsSortAscending), new ScalarValue(todoItemQuery.IsSortAscending))
                });

                return true;
            }

            result = null;
            return false;
        }
    }
}
