using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Bangumi.Api.Common
{
    public static class FileHelper
    {
        /// <summary>
        /// 加密委托
        /// </summary>
        internal static Func<string, Task<byte[]>> EncryptionAsync { get; set; }
        /// <summary>
        /// 解密委托
        /// </summary>
        internal static Func<byte[], Task<string>> DecryptionAsync { get; set; }

        #region 异步读写文件

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

        #endregion

        #region 加解密读写文件
        /// <summary>
        /// 加密并写入文件。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static async Task EncryptAndWriteFileAsync(string filePath, string data)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    using var f = File.Create(filePath);
                }
                var encryptedData = await EncryptionAsync(data);
                var tempFile = filePath + ".temp";
                using (var writer = File.Create(tempFile))
                {
                    await writer.WriteAsync(encryptedData, 0, encryptedData.Length).ConfigureAwait(false);
                }
                File.Replace(tempFile, filePath, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// 从文件读取并解密。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static async Task<string> ReadAndDecryptFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    byte[] encryptedData;
                    using (var reader = File.OpenRead(filePath))
                    {
                        encryptedData = new byte[reader.Length];
                        await reader.ReadAsync(encryptedData, 0, (int)reader.Length).ConfigureAwait(false);
                    }
                    return await DecryptionAsync(encryptedData).ConfigureAwait(false);
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return string.Empty;
            }
        }

        #endregion

        /// <summary>
        /// 获取文件长度 bytes
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static long GetFileLength(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        return fs.Length;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return 0;
            }
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
