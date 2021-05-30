namespace Todo.Integrations.Serilog.Destructuring
{
    using System.Collections.Generic;

    using global::Serilog.Core;
    using global::Serilog.Events;

    using Services.Security;
    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Instructs Serilog how to log instances of <seealso cref="DeleteTodoItemInfo"/> class.
    /// </summary>
    public class DeleteTodoItemInfoDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
            out LogEventPropertyValue result)
        {
            result = null;
            DeleteTodoItemInfo deleteTodoItemInfo = value as DeleteTodoItemInfo;

            if (deleteTodoItemInfo == null)
            {
                return false;
            }

            result = new StructureValue(new List<LogEventProperty>
            {
                new LogEventProperty(nameof(deleteTodoItemInfo.Id), new ScalarValue(deleteTodoItemInfo.Id)),
                new LogEventProperty(nameof(deleteTodoItemInfo.Owner),
                    new ScalarValue(deleteTodoItemInfo.Owner.GetNameOrDefault()))
            });

            return true;
        }
    }
}
