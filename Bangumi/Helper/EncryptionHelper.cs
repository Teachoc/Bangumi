using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace Bangumi.Helper
{
    public class EncryptionHelper
    {
        /// <summary>
        /// 加密字符串。
        /// </summary>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public static async Task<byte[]> EncryptionAsync(string strMsg)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider provider = new DataProtectionProvider("LOCAL=user");

            try
            {
                // Encode the plaintext input message to a buffer.
                IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, BinaryStringEncoding.Utf8);

                // Encrypt the message.
                IBuffer buffProtected = await provider.ProtectAsync(buffMsg);

                return buffProtected.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 解密字符串。
        /// </summary>
        /// <param name="buffProtected"></param>
        /// <returns></returns>
        public static async Task<string> DecryptionAsync(byte[] buffProtected)
        {
            // Create a DataProtectionProvider object.
            DataProtectionProvider provider = new DataProtectionProvider();

            try
            {
                // Decrypt the protected message specified on input.
                IBuffer buffUnprotected = await provider.UnprotectAsync(buffProtected.AsBuffer());

                // Convert the unprotected message from an IBuffer object to a string.
                string strClearText = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffUnprotected);

                // Return the plaintext string.
                return strClearText;
            }
            catch (Exception)
            {
                return "";
            }
        }

    }
}
