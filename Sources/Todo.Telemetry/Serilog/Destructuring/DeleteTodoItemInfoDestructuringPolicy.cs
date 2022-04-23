namespace Todo.Telemetry.Serilog.Destructuring
{
    using System.Collections.Generic;

    using global::Serilog.Core;
    using global::Serilog.Events;

    using Services.Security;
    using Services.TodoItemManagement;

    /// <summary>
    /// Instructs Serilog how to log instances of <see cref="DeleteTodoItemInfo"/> class.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class DeleteTodoItemInfoDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
            out LogEventPropertyValue result)
        {
            if (value is DeleteTodoItemInfo deleteTodoItemInfo)
            {
                result = new StructureValue(new List<LogEventProperty>
                {
                    new LogEventProperty(nameof(deleteTodoItemInfo.Id), new ScalarValue(deleteTodoItemInfo.Id)),
                    new LogEventProperty(nameof(deleteTodoItemInfo.Owner),
                        new ScalarValue(deleteTodoItemInfo.Owner.GetNameOrDefault()))
                });

                return true;
            }

            result = null;
            return false;
        }
    }
}
