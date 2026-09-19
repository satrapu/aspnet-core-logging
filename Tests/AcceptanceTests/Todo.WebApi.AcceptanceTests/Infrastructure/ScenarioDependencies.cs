namespace Todo.WebApi.AcceptanceTests.Infrastructure
{
    using Commons.Constants;
    using Drivers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Reqnroll.Microsoft.Extensions.DependencyInjection;
    using Services.Security;
    using System;
    using System.Net.Http.Headers;
    using System.Reflection;

    public static class ScenarioDependencies
    {
        private const string DefaultProductName = "Todo.WebApi.AcceptanceTests";
        private const string DefaultProductVersion = "1.0.0.0";

        [ScenarioDependencies]
        public static IServiceCollection CreateScenarioDependencies()
        {
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            ServiceCollection services = new();

            services
                .AddSingleton<IJwtService, JwtService>()
                .AddSingleton<TcpPortProvider>()
                .AddSingleton<TodoWebApiDriver>()
                .AddSingleton<IConfiguration>
                (
                    new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{EnvironmentNames.AcceptanceTests}.json", optional: false, reloadOnChange: false)
                        .AddEnvironmentVariables(prefix: EnvironmentVariables.Prefix)
                        .Build()
                )
                .AddHttpClient(name: TodoWebApiDriver.HttpClientName, (serviceProvider, httpClient) =>
                {
                    TcpPortProvider tcpPortProvider = serviceProvider.GetRequiredService<TcpPortProvider>();
                    int port = tcpPortProvider.GetAvailableTcpPort();

                    httpClient.BaseAddress = new Uri(uriString: $"http://localhost:{port}", uriKind: UriKind.Absolute);
                    httpClient.DefaultRequestHeaders.UserAgent.Add
                    (
                        item: new ProductInfoHeaderValue
                        (
                            product: new ProductHeaderValue
                            (
                                name: assemblyName.Name ?? DefaultProductName,
                                version: assemblyName.Version?.ToString() ?? DefaultProductVersion
                            )
                        )
                    );
                });

            return services;
        }
    }
}
