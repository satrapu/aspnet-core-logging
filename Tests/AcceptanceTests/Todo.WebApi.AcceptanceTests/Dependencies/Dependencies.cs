namespace Todo.WebApi.AcceptanceTests.Dependencies
{
    using System;
    using System.Net.Http.Headers;
    using System.Reflection;

    using Drivers;

    using Microsoft.Extensions.DependencyInjection;

    using SolidToken.SpecFlow.DependencyInjection;

    public static class Dependencies
    {
        private const string DefaultProductName = "Todo.WebApi.AcceptanceTests";
        private const string DefaultProductVersion = "1.0.0.0";

        [ScenarioDependencies]
        public static IServiceCollection CreateServices()
        {
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            ServiceCollection services = new();

            services
                .AddSingleton<TodoWebApiDriver>()
                .AddHttpClient(name: TodoWebApiDriver.HttpClientName, httpClient =>
                {
                    httpClient.BaseAddress = new Uri(uriString: "https://localhost:6001", uriKind: UriKind.Absolute);
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
