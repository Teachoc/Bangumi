using System.IO;
using System.Threading.Tasks;

namespace Bangumi.Data
{
    public static class FileHelper
    {
        /// <summary>
        /// 异步读文件，文件不存在将返回空字符串
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <returns></returns>
        public static async Task<string> ReadTextAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var reader = File.OpenText(filePath))
                {
                    return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 异步写文件
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <param name="data">待写入文本</param>
        /// <returns></returns>
        public static async Task WriteTextAsync(string filePath, string data)
        {
            if (!File.Exists(filePath))
            {
                using var f = File.Create(filePath);
            }
            var tempFile = filePath + ".temp";
            using (var writer = File.CreateText(tempFile))
            {
                await writer.WriteAsync(data).ConfigureAwait(false);
            }
            File.Replace(tempFile, filePath, null);
        }

        /// <summary>
        /// 删除存在的文件
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

    }
}
