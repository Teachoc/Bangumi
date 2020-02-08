using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Helper
{
    public static class FileSizeHelper
    {
        private static readonly string[] Unit = { "B", "KB", "MB" };

        public static string GetSizeString(double size)
        {
            for (int i = 0; i < Unit.Length; i++)
            {
                if (size < 1024 || i == Unit.Length - 1)
                {
                    return $"{size.ToString("F2")}{Unit[i]}";
                }
                size /= 1024;
            }
            return $"{size}B";
        }
    }
}
