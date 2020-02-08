using System;

namespace Bangumi.Api.Exceptions
{
    public class BgmTokenRefreshedException : Exception
    {
        public BgmTokenRefreshedException() : base()
        {
        }

        public BgmTokenRefreshedException(string message) : base(message)
        {
        }

        public BgmTokenRefreshedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
