using Bangumi.Api.Common;
using Bangumi.Api.Services;
using System;
using System.Threading.Tasks;

namespace Bangumi.Api
{
    /// <summary>
    /// 提供 Api 访问以及缓存管理
    /// </summary>
    public class BangumiApi
    {
        private static IBgmCache _bgmCache;
        private static IBgmOAuth _bgmOAuth;
        private static IBgmApi _bgmApi;
        public static IBgmCache BgmCache { get => _bgmCache ?? throw new NullReferenceException("Not initialized."); }
        public static IBgmOAuth BgmOAuth { get => _bgmOAuth ?? throw new NullReferenceException("Not initialized."); }
        public static IBgmApi BgmApi { get => _bgmApi ?? throw new NullReferenceException("Not initialized."); }

        /// <summary>
        /// 初始化 Api
        /// </summary>
        /// <param name="localFolder">本地文件夹</param>
        /// <param name="cacheFolder">缓存文件夹</param>
        /// <param name="encryptionDelegate">加密方法</param>
        /// <param name="decryptionDelegate">解密方法</param>
        public static void Init(
            string localFolder,
            string cacheFolder,
            Func<string, Task<byte[]>> encryptionDelegate,
            Func<byte[], Task<string>> decryptionDelegate)
        {
            FileHelper.EncryptionAsync = encryptionDelegate ?? throw new ArgumentNullException(nameof(encryptionDelegate));
            FileHelper.DecryptionAsync = decryptionDelegate ?? throw new ArgumentNullException(nameof(decryptionDelegate));
            if (_bgmCache == null && _bgmApi == null)
            {
                _bgmCache = new BgmCache(cacheFolder);
                _bgmOAuth = new BgmOAuth(localFolder, _bgmCache);
                _bgmApi = new BgmApi(_bgmCache, _bgmOAuth);
            }
            else
            {
                throw new InvalidOperationException("BangumiApi can't init twice!");
            }
        }
    }
}
