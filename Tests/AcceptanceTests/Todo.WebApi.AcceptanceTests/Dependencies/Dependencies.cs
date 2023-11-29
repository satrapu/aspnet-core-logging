namespace Todo.WebApi.AcceptanceTests.Dependencies
{
    using System;
    using System.Net.Http.Headers;
    using System.Reflection;

    using Microsoft.Extensions.DependencyInjection;

    using SolidToken.SpecFlow.DependencyInjection;

    using Todo.WebApi.AcceptanceTests.Drivers;

    public static class Dependencies
    {
        [ScenarioDependencies]
        public static IServiceCollection CreateServices()
        {
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();

            ProductHeaderValue productHeaderValue = new ProductHeaderValue
            (
                name: assemblyName.Name ?? "Todo.WebApi.AcceptanceTests",
                version: assemblyName.Version?.ToString() ?? "1.0.0.0"
            );

            ServiceCollection services = new();

            services
                .AddSingleton<TodoWebApiDriver>()
                .AddHttpClient(name: "AcceptanceTests", httpClient =>
                {
                    httpClient.BaseAddress = new Uri(uriString: "https://localhost:5001", uriKind: UriKind.Absolute);
                    httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(productHeaderValue));
                });

            return services;
        }
    }
}
