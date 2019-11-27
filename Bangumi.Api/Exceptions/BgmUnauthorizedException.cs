using System;

namespace Bangumi.Api.Exceptions
{
    public class BgmUnauthorizedException : Exception
    {
        public BgmUnauthorizedException() : base()
        {

        }
        public BgmUnauthorizedException(string message) : base(message)
        {
        }

        public BgmUnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
