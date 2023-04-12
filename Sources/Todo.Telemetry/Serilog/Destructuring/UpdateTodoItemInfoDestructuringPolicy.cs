namespace Todo.Telemetry.Serilog.Destructuring
{
    using System.Collections.Generic;

    using global::Serilog.Core;
    using global::Serilog.Events;

    using Services.Security;
    using Services.TodoItemManagement;

    /// <summary>
    /// Instructs Serilog how to log instances of <see cref="UpdateTodoItemInfo"/> class.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class UpdateTodoItemInfoDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
            out LogEventPropertyValue result)
        {
            if (value is UpdateTodoItemInfo updateTodoItemInfo)
            {
                result = new StructureValue(new List<LogEventProperty>
                {
                    new(nameof(updateTodoItemInfo.Id), new ScalarValue(updateTodoItemInfo.Id)),
                    new(nameof(updateTodoItemInfo.Name), new ScalarValue(updateTodoItemInfo.Name)),
                    new(nameof(updateTodoItemInfo.IsComplete), new ScalarValue(updateTodoItemInfo.IsComplete)),
                    new(nameof(updateTodoItemInfo.Owner), new ScalarValue(updateTodoItemInfo.Owner.GetNameOrDefault()))
                });

                return true;
            }

            result = null;
            return false;
        }
    }
}
