using System.Diagnostics.CodeAnalysis;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Todo.Commons.Constants;
using Todo.Commons.StartupLogic;
using Todo.WebApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: EnvironmentVariables.Prefix)
    .AddCommandLine(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

Startup startup = new(configuration: builder.Configuration, webHostEnvironment: builder.Environment);
startup.ConfigureServices(builder.Services);

builder.Host.ConfigureContainer<ContainerBuilder>(startup.ConfigureContainer);

WebApplication app = builder.Build();

startup.Configure
(
    applicationBuilder: app,
    hostApplicationLifetime: app.Services.GetRequiredService<IHostApplicationLifetime>(),
    serviceProvider: app.Services,
    logger: app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Startup>()
);

await app.RunWithTasksAsync();

[SuppressMessage("Sonar", "S1118", Justification = "Class is needed by WebApplicationFactory")]
[ExcludeFromCodeCoverage]
public partial class Program
{
}
