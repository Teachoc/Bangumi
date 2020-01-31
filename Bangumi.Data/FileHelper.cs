using System.IO;
using System.Threading.Tasks;

namespace Bangumi.Data
{
    internal static class FileHelper
    {
        /// <summary>
        /// 异步读文件
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <returns></returns>
        internal static async Task<string> ReadTextAsync(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 异步写文件
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <param name="data">待写入文本</param>
        /// <returns></returns>
        internal static async Task WriteTextAsync(string filePath, string data)
        {
            using (var writer = File.CreateText(filePath))
            {
                await writer.WriteAsync(data).ConfigureAwait(false);
            }
        }

    }
}
