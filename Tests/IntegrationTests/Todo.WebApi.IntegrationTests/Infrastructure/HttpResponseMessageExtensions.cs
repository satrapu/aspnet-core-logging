using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Todo.WebApi.Infrastructure
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task PrintError(this HttpResponseMessage response, ITestOutputHelper testOutputHelper)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine("Status code: {0}", response.StatusCode);
            testOutputHelper.WriteLine("Reason phrase: {0}", response.ReasonPhrase);
            testOutputHelper.WriteLine("Response content: {0}", responseContent);
        }
    }
}
