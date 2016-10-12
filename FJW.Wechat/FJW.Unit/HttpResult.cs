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

    public class HttpResult<T> : HttpResult where T : class
    {
        public HttpResult()
        {
            
        }

        public T Data { get; set; }

        public bool IsOk { get; set; }

       

        public HttpResult(HttpResult result)
        {
            
            Reponse = result.Reponse;
        }
    }
}