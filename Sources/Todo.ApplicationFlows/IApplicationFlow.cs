namespace Todo.ApplicationFlows
{
    using System.Security.Principal;
    using System.Threading.Tasks;

    /// <summary>
    /// An application flow implements a specific feature needed for business or technical reasons.
    /// </summary>
    /// <typeparam name="TInput">The type of the input needed to execute this flow.</typeparam>
    /// <typeparam name="TOutput">The type of the outcome of this flow.</typeparam>
    public interface IApplicationFlow<in TInput, TOutput>
    {
        /// <summary>
        /// Executes this flow.
        /// </summary>
        /// <param name="input">The input needed to execute this flow.</param>
        /// <param name="flowInitiator">The user who initiated executing this flow.</param>
        /// <returns>An instance of <see cref="TOutput"/> type.</returns>
        Task<TOutput> ExecuteAsync(TInput input, IPrincipal flowInitiator);
    }
}
