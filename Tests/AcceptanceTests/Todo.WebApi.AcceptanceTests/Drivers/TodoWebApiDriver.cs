namespace Todo.WebApi.AcceptanceTests.Drivers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    public class TodoWebApiDriver
    {
        private readonly HttpClient httpClient;

        public TodoWebApiDriver(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient(name: "AcceptanceTests");
        }

        public async Task<HttpResponseMessage> AddNewTodoItemAsync(NewTodoItemInfo newTodoItemInfo)
        {
            using HttpRequestMessage httpRequestMessage = new(method: HttpMethod.Post, requestUri: "api/todo");
            httpRequestMessage.Content = JsonContent.Create(newTodoItemInfo);

            await InjectAuthorizationHeaderAsync(httpRequestMessage);

            return await httpClient.SendAsync(httpRequestMessage);
        }

        private async Task InjectAuthorizationHeaderAsync(HttpRequestMessage httpRequestMessage)
        {
            using HttpRequestMessage jwtHttpRequestMessage = new(method: HttpMethod.Post, requestUri: "api/jwt");

            jwtHttpRequestMessage.Content = JsonContent.Create(new
            {
                UserName = "acceptance-tests-user",
                Password = "Qwerty!"
            });

            using HttpResponseMessage jwtHttpResponseMessage = await httpClient.SendAsync(jwtHttpRequestMessage);
            jwtHttpResponseMessage.EnsureSuccessStatusCode();

            Type resultType = new { accessToken = "" }.GetType();
            dynamic dynamicResult = await jwtHttpResponseMessage.Content.ReadAsAsync(resultType);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(scheme: "Bearer", parameter: dynamicResult.accessToken);
        }
    }
}
