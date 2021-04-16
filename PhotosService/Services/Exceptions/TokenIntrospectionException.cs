using System;

namespace PhotosService.Services.Exceptions
{
    public class TokenIntrospectionException : Exception
    {
        public TokenIntrospectionException(string message) : base(message)
        {
        }
    }
}