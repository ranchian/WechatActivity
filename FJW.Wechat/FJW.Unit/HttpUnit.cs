using System.IO;
using System.Net;
using System.Text;


namespace FJW.Unit
{
    public class HttpUnit
    {

        private static IWebProxy _webProxy;

        public static void SetWebProxy(string host, string port, string username, string password)
        {
            ICredentials credentials = new NetworkCredential(username, password);
            if (!string.IsNullOrEmpty(host))
            {
                _webProxy = new WebProxy(host + ":" + (port ?? "80"), true, null, credentials);
            }
        }

        public static  HttpResult Post(string url, string data, Encoding code = null, string contentType = "application/x-www-form-urlencoded")
        {
            code = code ?? Encoding.UTF8;
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/x-www-form-urlencoded";
            }
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType+";charset=" + code.WebName;

            using (var writer = new StreamWriter(request.GetRequestStream(), code))
            {
                writer.Write(data);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var resp = string.Empty;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream, code))
                        resp = reader.ReadToEnd();
                }
            }
            return new HttpResult(response.StatusCode, resp);
        }



   
        public static string GetString(string url, Encoding encoding = null)
        {
            return new WebClient
            {
                Proxy = _webProxy,
                Encoding = encoding ?? Encoding.UTF8
            }.DownloadString(url);
        }

        public static byte[] GetData(string url)
        {
            return new WebClient
            {
                Proxy = _webProxy
            }.DownloadData(url);
        }
    }
}