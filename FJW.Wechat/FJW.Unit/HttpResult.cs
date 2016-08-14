using System.Net;

namespace FJW.Unit
{
    public class HttpResult
    {

        public HttpResult()
        {

        }

        public HttpResult(HttpStatusCode statusCode, string resp)
        {
            this.Code = statusCode;
            this.Reponse = resp;
        }

        public HttpStatusCode Code { get; set; }

        public string Reponse { get; set; }
    }
}