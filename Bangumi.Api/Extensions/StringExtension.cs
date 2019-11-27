using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Extensions
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}
