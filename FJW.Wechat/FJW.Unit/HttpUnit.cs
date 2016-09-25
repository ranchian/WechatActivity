using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace FJW.Unit
{
    public class HttpUnit
    {
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<HttpResult> Post(string url, object data, Encoding code,
            string mediaType = "application/json")
        {
            using (var byteContent = new StringContent(data.ToJson(), code, mediaType))
            {
                if (byteContent.Headers.ContentType == null ||
                    byteContent.Headers.ContentType.MediaType != mediaType)
                {
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                }
                var result = await Client.PostAsync(url, byteContent);
                if (result.IsSuccessStatusCode)
                {
                    var resp = await result.Content.ReadAsStringAsync();
                    return new HttpResult(result.StatusCode, resp);
                }
                return new HttpResult(result.StatusCode, string.Empty);
            }
        }
    }
}