using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Bangumi.Api
{
    public static class FileHelper
    {
        public delegate Task<byte[]> EncryptionDelegate(string data);
        public delegate Task<string> DecryptionDelegate(byte[] buff);

        /// <summary>
        /// 加密委托
        /// </summary>
        internal static EncryptionDelegate EncryptionAsync { get; set; }
        /// <summary>
        /// 解密委托
        /// </summary>
        internal static DecryptionDelegate DecryptionAsync { get; set; }

        #region 异步读写文件

        /// <summary>
        /// 异步读文件
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <returns></returns>
        internal static async Task<string> ReadTextAsync(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                return await reader.ReadToEndAsync();
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
                await writer.WriteAsync(data);
            }
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
                var encryptedData = await EncryptionAsync(data);
                using (var writer = File.Create(filePath))
                {
                    await writer.WriteAsync(encryptedData, 0, encryptedData.Length);
                }
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
                        await reader.ReadAsync(encryptedData, 0, (int)reader.Length);
                    }
                    return await DecryptionAsync(encryptedData);
                }
                return "";
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return "";
            }
        }

        #endregion

        /// <summary>
        /// 获取文件长度 bytes
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static long GetFileLength(string filePath)
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
        internal static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

    }
}
