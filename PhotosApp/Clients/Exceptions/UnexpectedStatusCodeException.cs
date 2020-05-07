using System;
using System.Net;

namespace PhotosApp.Clients.Exceptions
{
    public class UnexpectedStatusCodeException : Exception
    {
        public UnexpectedStatusCodeException(HttpStatusCode statusCode)
            : base($"Unexpected status code '{statusCode}' was returned")
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
