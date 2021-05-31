namespace Todo.Integrations.Serilog.Destructuring
{
    using System.Collections.Generic;

    using global::Serilog.Core;
    using global::Serilog.Events;

    using Services.Security;
    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Instructs Serilog how to log instances of <seealso cref="UpdateTodoItemInfo"/> class.
    /// </summary>
    public class UpdateTodoItemInfoDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
            out LogEventPropertyValue result)
        {
            result = null;
            UpdateTodoItemInfo updateTodoItemInfo = value as UpdateTodoItemInfo;

            if (updateTodoItemInfo == null)
            {
                return false;
            }

            result = new StructureValue(new List<LogEventProperty>
            {
                new LogEventProperty(nameof(updateTodoItemInfo.Id), new ScalarValue(updateTodoItemInfo.Id)),
                new LogEventProperty(nameof(updateTodoItemInfo.Name), new ScalarValue(updateTodoItemInfo.Name)),
                new LogEventProperty(nameof(updateTodoItemInfo.IsComplete), new ScalarValue(updateTodoItemInfo.IsComplete)),
                new LogEventProperty(nameof(updateTodoItemInfo.Owner),
                    new ScalarValue(updateTodoItemInfo.Owner.GetNameOrDefault()))
            });

            return true;
        }
    }
}
