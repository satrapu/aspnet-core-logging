namespace Todo.WebApi.ExceptionHandling
{
    using System.Net;

    public sealed record ExceptionMappingResult(HttpStatusCode HttpStatusCode, string RootCauseKey);
}
