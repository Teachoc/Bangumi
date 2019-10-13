using Bangumi.Api.Models;
using Bangumi.Api.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Timers;

namespace Bangumi.Api
{
    /// <summary>
    /// 提供 Api 访问以及缓存管理
    /// </summary>
    public static partial class BangumiApi
    {
        /// <summary>
        /// 初始化 Api
        /// </summary>
        /// <param name="localFolder">本地文件夹</param>
        /// <param name="cacheFolder">缓存文件夹</param>
        /// <param name="baseUrl"></param>
        /// <param name="oAuthBaseUrl"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="redirectUrl"></param>
        /// <param name="noImageUri">无图片时显示的图片路径</param>
        /// <param name="encryptionDelegate">加密方法</param>
        /// <param name="decryptionDelegate">解密方法</param>
        /// <param name="checkNetworkActivityDelegate">检查网络是否可用的方法</param>
        public static async void Init(
            string localFolder,
            string cacheFolder,
            string baseUrl,
            string oAuthBaseUrl,
            string clientId,
            string clientSecret,
            string redirectUrl,
            string noImageUri,
            FileHelper.EncryptionDelegate encryptionDelegate,
            FileHelper.DecryptionDelegate decryptionDelegate,
            CheckNetworkDelegate checkNetworkActivityDelegate)
        {

            FileHelper.EncryptionAsync = encryptionDelegate ?? throw new ArgumentNullException(nameof(encryptionDelegate));
            FileHelper.DecryptionAsync = decryptionDelegate ?? throw new ArgumentNullException(nameof(decryptionDelegate));
            _checkNetworkAction = checkNetworkActivityDelegate ?? throw new ArgumentNullException(nameof(checkNetworkActivityDelegate));
            _isOffline = _checkNetworkAction();
            if (_wrapper == null && BangumiCache == null && _timer == null)
            {
                _localFolderPath = localFolder ?? throw new ArgumentNullException(nameof(localFolder));
                _cacheFolderPath = cacheFolder ?? throw new ArgumentNullException(nameof(cacheFolder));
                _wrapper = new BangumiHttpWrapper
                {
                    BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl)),
                    OAuthBaseUrl = oAuthBaseUrl ?? throw new ArgumentNullException(nameof(oAuthBaseUrl)),
                    ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId)),
                    ClientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret)),
                    RedirectUrl = redirectUrl ?? throw new ArgumentNullException(nameof(redirectUrl)),
                    NoImageUri = noImageUri ?? throw new ArgumentNullException(nameof(noImageUri))
                };

                // 加载缓存
                BangumiCache = new BangumiCache();
                if (File.Exists(AppFile.BangumiCache.GetFilePath(_cacheFolderPath)))
                {
                    try
                    {
                        BangumiCache = JsonConvert.DeserializeObject<BangumiCache>(await FileHelper.ReadTextAsync(AppFile.BangumiCache.GetFilePath(_cacheFolderPath)));
                    }
                    catch (Exception)
                    {
                        FileHelper.DeleteFile(AppFile.BangumiCache.GetFilePath(_cacheFolderPath));
                    }
                }

                // 启动定时器，定时将缓存写入文件，30 秒
                _timer = new Timer(_interval);
                _timer.Elapsed += WriteCacheToFileTimer_Elapsed;
                _timer.AutoReset = true;
                _timer.Start();
            }
            else
            {
                throw new InvalidOperationException("BangumiApi can't init twice!");
            }
        }


        #region AppFile
        /// <summary>
        /// 使用的文件
        /// </summary>
        private enum AppFile
        {
            Token_Data,
            BangumiCache,
        }

        /// <summary>
        /// 文件名转换为小写，
        /// 与文件夹组合为路径，
        /// 将 '_' 替换为 '.'
        /// </summary>
        /// <param name="file"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static string GetFilePath(this AppFile file, string folder)
        {
            return Path.Combine(folder, file.ToString().ToLower().Replace('_', '.'));
        }

        #endregion


    }
}
