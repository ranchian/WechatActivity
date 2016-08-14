
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace FJW.Unit
{
    public class HttpUnit
    {
        public static HttpResult Post(string url, object data, Encoding code, string mediaType = "application/json")
        {
            using (var client = new HttpClient())
            using (var byteContent = new StringContent(data.ToJson(), code, mediaType))
            {
                if (byteContent.Headers.ContentType == null || byteContent.Headers.ContentType.MediaType != mediaType)
                {
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                }
                var result = client.PostAsync(url, byteContent).Result;
                if (result.IsSuccessStatusCode)
                {
                    var resp = result.Content.ReadAsStringAsync().Result;
                    return new HttpResult(result.StatusCode, resp);
                }
                return new HttpResult(result.StatusCode, "");
            }
        }
    }
}
