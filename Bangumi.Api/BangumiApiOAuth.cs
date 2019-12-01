using Bangumi.Api.Exceptions;
using Bangumi.Api.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bangumi.Api
{
    /// <summary>
    /// BangumiApi 的 OAuth 部分
    /// </summary>
    public static partial class BangumiApi
    {
        /// <summary>
        /// 本地文件夹路径，永久保存
        /// </summary>
        private static string _localFolderPath;

        /// <summary>
        /// 用来表示 Token 过期或不可用
        /// </summary>
        private static bool _isLogin;

        /// <summary>
        /// 用户认证存在且可用
        /// </summary>
        public static bool IsLogin { get => MyToken != null && _isLogin; }

        /// <summary>
        /// 存储 Token
        /// </summary>
        public static AccessToken MyToken { get; private set; }

        public static string OAuthBaseUrl => _wrapper.OAuthBaseUrl;
        public static string ClientId => _wrapper.ClientId;
        public static string RedirectUrl => _wrapper.RedirectUrl;


        #region public
        /// <summary>
        /// 使用 code 换取 Access Token。
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task GetTokenAsync(string code)
        {
            AccessToken token;
            // 重试最多三次
            for (int i = 0; i < 3; i++)
            {
                Debug.WriteLine($"第{i + 1}次尝试获取Token。");
                token = await _wrapper.GetTokenAsync(code);
                if (token != null)
                {
                    await WriteTokenAsync(token);
                    break;
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        /// <summary>
        /// 检查用户授权文件。
        /// </summary>
        /// <returns></returns>
        public static async Task<(bool, Task)> CheckMyToken()
        {
            if (MyToken == null)
            {
                MyToken = JsonConvert.DeserializeObject<AccessToken>(await FileHelper.ReadAndDecryptFileAsync(AppFile.Token_data.GetFilePath(_localFolderPath)));
                if (MyToken == null)
                {
                    //DeleteTokens();
                    _isLogin = false;
                }
                else
                {
                    _isLogin = true;
                }
            }
            return (_isLogin, CheckToken());
        }

        /// <summary>
        /// 删除用户相关文件。
        /// </summary>
        /// <returns></returns>
        public static void DeleteToken()
        {
            // 删除用户认证文件
            MyToken = null;
            FileHelper.DeleteFile(AppFile.Token_data.GetFilePath(_localFolderPath));
            // 清空用户缓存
            DeleteCache();
        }
        #endregion


        #region private
        /// <summary>
        /// 查询授权信息，并在满足条件时刷新Token。
        /// </summary>
        private static async Task CheckToken()
        {
            try
            {
                Debug.WriteLine("尝试刷新Token。");
                var token = await _wrapper.CheckTokenAsync(MyToken);
                if (token != null)
                {
                    // 将信息写入本地文件
                    if (!token.EqualsExT(MyToken))
                        await WriteTokenAsync(token);
                }
            }
            catch (BgmUnauthorizedException)
            {
                _isLogin = false;
                throw;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                RecheckNetworkStatus();
            }
        }

        /// <summary>
        /// 将 Token 写入内存及文件。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task WriteTokenAsync(AccessToken token)
        {
            // 存入内存
            MyToken = token;
            _isLogin = true;
            // 将信息写入本地文件
            await FileHelper.EncryptAndWriteFileAsync(AppFile.Token_data.GetFilePath(_localFolderPath),
                                                      JsonConvert.SerializeObject(token));
        }
        #endregion
    }
}
