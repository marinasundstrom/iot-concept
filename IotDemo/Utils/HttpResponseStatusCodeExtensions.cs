using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDemo.Utils
{
    public static class HttpResponseStatusCodeExtensions
    {
        public static void NotFound(this HttpServerResponse response)
        {
            response.StatusCode = 401;
            response.StatusDescription = "Not Found";
        }

        public static void InternalServerError(this HttpServerResponse response)
        {
            response.StatusCode = 500;
            response.StatusDescription = "Internal Server Error";
        }

        public static void MethodNotAllowed(this HttpServerResponse response)
        {
            response.StatusCode = 405;
            response.StatusDescription = "Method Not Allowed";
        }

        public static void NotImplemented(this HttpServerResponse response)
        {
            response.StatusCode = 501;
            response.StatusDescription = "Not Implemented";
        }

        public static void Forbidden(this HttpServerResponse response)
        {
            response.StatusCode = 403;
            response.StatusDescription = "Forbidden";
        }

    }
}
