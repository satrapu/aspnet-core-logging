namespace Todo.WebApi.AcceptanceTests.Drivers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    public class TodoWebApiDriver
    {
        private const string AuthenticationScheme = "Bearer";
        private static readonly Type accessTokenType = new { accessToken = "" }.GetType();
        private readonly HttpClient httpClient;

        public TodoWebApiDriver(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient(name: "AcceptanceTests");
        }

        public async Task<HttpResponseMessage> AddNewTodoItemAsync
        (
            NewTodoItemInfo newTodoItemInfo,
            AuthenticationHeaderValue authenticationHeaderValue
        )
        {
            using HttpRequestMessage httpRequestMessage = new(method: HttpMethod.Post, requestUri: "api/todo");
            httpRequestMessage.Content = JsonContent.Create(newTodoItemInfo);
            httpRequestMessage.Headers.Authorization = authenticationHeaderValue;

            return await httpClient.SendAsync(httpRequestMessage);
        }


        public async Task<AuthenticationHeaderValue> GetAuthorizationHeaderAsync(UserDetails userDetails)
        {
            using HttpRequestMessage httpRequestMessage = new(method: HttpMethod.Post, requestUri: "api/jwt");
            httpRequestMessage.Content = JsonContent.Create(userDetails);

            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            httpResponseMessage.EnsureSuccessStatusCode();
            dynamic dynamicResult = await httpResponseMessage.Content.ReadAsAsync(type: accessTokenType);

            return new AuthenticationHeaderValue(scheme: AuthenticationScheme, parameter: dynamicResult.accessToken);
        }
    }
}