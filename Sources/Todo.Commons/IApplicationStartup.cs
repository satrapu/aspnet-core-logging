namespace Todo.Commons
{
    /// <summary>
    /// Allows services to execute logic application startup.
    /// </summary>
    public interface IApplicationStartup
    {
        public void Execute();
    }
}
