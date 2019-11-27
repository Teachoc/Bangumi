using System;

namespace Bangumi.Api.Exceptions
{
    public class BgmTimeoutException : Exception
    {
        public BgmTimeoutException() : base()
        {
        }

        public BgmTimeoutException(string message) : base(message)
        {
        }

        public BgmTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
