namespace Todo.Logging.Serilog.Destructuring
{
    using System.Collections.Generic;

    using global::Serilog.Core;
    using global::Serilog.Events;

    using Services.Security;
    using Services.TodoItemManagement;

    /// <summary>
    /// Instructs Serilog how to log instances of <seealso cref="NewTodoItemInfo"/> class.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class NewTodoItemInfoDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
            out LogEventPropertyValue result)
        {
            if (value is NewTodoItemInfo newTodoItemInfo)
            {
                result = new StructureValue(new List<LogEventProperty>
                {
                    new LogEventProperty(nameof(newTodoItemInfo.Name), new ScalarValue(newTodoItemInfo.Name)),
                    new LogEventProperty(nameof(newTodoItemInfo.Owner),
                        new ScalarValue(newTodoItemInfo.Owner.GetNameOrDefault()))
                });

                return true;
            }

            result = null;
            return false;
        }
    }
}
