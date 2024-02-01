namespace Todo.WebApi.AcceptanceTests.Drivers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    using Services.Security;

    public class TodoWebApiDriver
    {
        private static readonly Type AccessTokenType = new { accessToken = "" }.GetType();

        internal const string HttpClientName = nameof(TodoWebApiDriver);
        private const string AuthenticationScheme = "Bearer";

        private readonly HttpClient httpClient;

        public TodoWebApiDriver(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient(name: HttpClientName);
        }

        public async Task<HttpResponseMessage> AddNewTodoItemAsync(NewTodoItemInfo newTodoItemInfo, AuthenticationHeaderValue authenticationHeaderValue)
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
            dynamic dynamicResult = await httpResponseMessage.Content.ReadAsAsync(type: AccessTokenType);

            return new AuthenticationHeaderValue(scheme: AuthenticationScheme, parameter: dynamicResult.accessToken);
        }

        public async Task<AuthenticationHeaderValue> GetAuthorizationHeaderAsync(UserDetails userDetails, string[] scopes)
        {
            JwtService jwtService = new();

            JwtInfo jwtInfo = await jwtService.GenerateJwtAsync
            (
                new GenerateJwtInfo
                {
                    UserName = userDetails.UserName,
                    Password = userDetails.Password,
                    Scopes = scopes,
                    Issuer = "https://acceptancetests.auth.todo-by-satrapu.com",
                    Audience = "https://acceptancetests.api.todo-by-satrapu.com",
                    Secret = Guid.NewGuid().ToString("N")
                }
            );

            return new AuthenticationHeaderValue(scheme: AuthenticationScheme, parameter: jwtInfo.AccessToken);
        }
    }
}
