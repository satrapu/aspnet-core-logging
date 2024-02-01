namespace Todo.WebApi.AcceptanceTests.Infrastructure
{
    using System;
    using System.Net.Http.Headers;
    using System.Reflection;

    using Microsoft.Extensions.DependencyInjection;

    using SolidToken.SpecFlow.DependencyInjection;

    using Drivers;

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
                .AddSingleton<JwtSecretProvider>()
                .AddSingleton<TcpPortProvider>()
                .AddSingleton<TodoWebApiDriver>()
                .AddHttpClient(name: TodoWebApiDriver.HttpClientName, (serviceProvider, httpClient) =>
                {
                    TcpPortProvider tcpPortProvider = serviceProvider.GetRequiredService<TcpPortProvider>();
                    int port = tcpPortProvider.GetAvailableTcpPort();

                    httpClient.BaseAddress = new Uri(uriString: $"http://localhost:{port}", uriKind: UriKind.Absolute);
                    httpClient.DefaultRequestHeaders.UserAgent.Add
                    (
                        new ProductInfoHeaderValue
                        (
                            new ProductHeaderValue
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
