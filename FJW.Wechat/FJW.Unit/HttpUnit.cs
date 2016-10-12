using System.IO;
using System.Net;
using System.Text;


namespace FJW.Unit
{
    public class HttpUnit
    {
        
        public static  HttpResult Post(string url, string data, Encoding code)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            //request.Timeout = readTimeoutMs;
            //request.ReadWriteTimeout = readTimeoutMs;
            //request.ContentType = "application/x-www-form-urlencoded;charset=" + code.WebName;

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
    }
}