namespace Todo.Integrations.Serilog.Destructuring
{
    using System.Collections.Generic;

    using global::Serilog.Core;
    using global::Serilog.Events;

    using Services.Security;
    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Instructs Serilog how to log instances of <seealso cref="NewTodoItemInfo"/> class.
    /// </summary>
    public class NewTodoItemInfoDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
            out LogEventPropertyValue result)
        {
            result = null;
            NewTodoItemInfo newTodoItemInfo = value as NewTodoItemInfo;

            if (newTodoItemInfo == null)
            {
                return false;
            }

            result = new StructureValue(new List<LogEventProperty>
            {
                new LogEventProperty(nameof(newTodoItemInfo.Name), new ScalarValue(newTodoItemInfo.Name)),
                new LogEventProperty(nameof(newTodoItemInfo.Owner),
                    new ScalarValue(newTodoItemInfo.Owner.GetNameOrDefault()))
            });

            return true;
        }
    }
}
