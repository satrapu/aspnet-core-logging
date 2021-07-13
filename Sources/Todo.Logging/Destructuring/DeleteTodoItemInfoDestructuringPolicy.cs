namespace Todo.Logging.Destructuring
{
    using System.Collections.Generic;

    using Serilog.Core;
    using Serilog.Events;

    using Services.Security;
    using Services.TodoItemManagement;

    /// <summary>
    /// Instructs Serilog how to log instances of <seealso cref="DeleteTodoItemInfo"/> class.
    /// </summary>
    // ReSharper disable once UnusedType.Global
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
