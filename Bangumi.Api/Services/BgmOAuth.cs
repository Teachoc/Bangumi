using Bangumi.Api.Common;
using Bangumi.Api.Exceptions;
using Bangumi.Api.Models;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Bangumi.Api.Services
{
    public class BgmOAuth : IBgmOAuth
    {
        public const string OAuthHOST = "https://bgm.tv/oauth";
        // 将自己申请的应用相关信息填入
        public const string ClientId = "bgm8905c514a1b94ec1";
        public const string ClientSecret = "b678c34dd896203627da308b6b453775";
        public const string RedirectUrl = "BangumiGithubVersion";

        private const int RetryCount = 3;
        private readonly string _localFolder;
        private readonly IBgmCache _bgmCache;
        private bool IsTokenChecking;

        public bool IsLogin => MyToken?.Expires > DateTime.Now.ToJsTick();

        public AccessToken MyToken { get; private set; }

        public BgmOAuth(string localFolder, IBgmCache bgmCache)
        {
            _localFolder = localFolder ?? throw new ArgumentNullException(nameof(localFolder));
            _bgmCache = bgmCache ?? throw new ArgumentNullException(nameof(bgmCache));

            Task.Run(async () =>
            {
                MyToken = JsonConvert.DeserializeObject<AccessToken>(await FileHelper.ReadAndDecryptFileAsync(AppFile.Token_data.GetFilePath(_localFolder)));
            }).Wait();

            FlurlHttp.ConfigureClient(OAuthHOST, client =>
            {
                client.Settings.BeforeCall = call =>
                {
                    if (IsLogin)
                    {
                        call.Request.Headers.Add("Authorization", $"Bearer {MyToken.Token}");
                    }
                    call.Request.Headers.Add("Cookie", $"chii_searchDateLine={DateTime.Now.ToString()}");
                };
            });
        }

        public async Task GetToken(string code)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    Debug.WriteLine($"第{i + 1}次尝试获取Token。");
                    MyToken = await $"{OAuthHOST}/access_token"
                        .PostUrlEncodedAsync(new
                        {
                            grant_type = "authorization_code",
                            client_id = ClientId,
                            client_secret = ClientSecret,
                            code,
                            redirect_uri = RedirectUrl
                        })
                        .ReceiveJson<AccessToken>();
                    MyToken.Expires = DateTime.Now.AddSeconds(MyToken.ExpiresIn).ToJsTick();
                    await SaveToken().ConfigureAwait(false);
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"第{i + 1}次尝试获取Token失败。");
                    Debug.WriteLine(e.StackTrace);
                    await Task.Delay(1000).ConfigureAwait(false);
                    if (i + 1 >= RetryCount)
                    {
                        throw;
                    }
                }
            }
            if (MyToken == null)
            {
                throw new BgmUnauthorizedException();
            }
        }

        public async Task CheckToken()
        {
            if (MyToken == null)
            {
                throw new BgmUnauthorizedException();
            }

            if (IsTokenChecking)
            {
                return;
            }
            IsTokenChecking = true;
            Debug.WriteLine("检查 Token 有效期。");
            try
            {
                var tokenStatus = await $"{OAuthHOST}/token_status"
                    .PostStringAsync(string.Empty)
                    .ReceiveJson<AccessToken>();
                // 若token对应的用户ID不同，
                // 获取1天后的时间戳，离过期不足1天时或过期后，
                // 更新 access_token
                if (tokenStatus.UserId != MyToken.UserId
                    || tokenStatus.Expires < DateTime.Now.AddDays(1).ToJsTick())
                {
                    await RefreshToken().ConfigureAwait(false);
                }
            }
            catch (FlurlHttpException e)
            {
                if (e.Call.HttpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await RefreshToken().ConfigureAwait(false);
                }
                else if (e.Call.HttpResponseMessage.StatusCode == HttpStatusCode.BadRequest
                         && (await e.Call.HttpResponseMessage.Content.ReadAsStringAsync()).Contains("Invalid refresh token"))
                {
                    throw new BgmUnauthorizedException();
                }
            }
            finally
            {
                IsTokenChecking = false;
            }
        }

        public void DeleteUserFiles()
        {
            // 删除用户认证文件
            MyToken = null;
            FileHelper.DeleteFile(AppFile.Token_data.GetFilePath(_localFolder));
            // 清空缓存
            _bgmCache.Delete();
        }

        private async Task RefreshToken()
        {
            if (MyToken == null)
            {
                throw new BgmUnauthorizedException();
            }

            Debug.WriteLine("刷新Token。");
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    MyToken = await $"{OAuthHOST}/access_token"
                        .PostUrlEncodedAsync(new
                        {
                            grant_type = "refresh_token",
                            client_id = ClientId,
                            client_secret = ClientSecret,
                            refresh_token = MyToken.RefreshToken,
                            redirect_uri = RedirectUrl
                        })
                        .ReceiveJson<AccessToken>();
                    MyToken.Expires = DateTime.Now.AddSeconds(MyToken.ExpiresIn).ToJsTick();
                    await SaveToken().ConfigureAwait(false);
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"第{i + 1}次尝试获取Token失败。");
                    Debug.WriteLine(e.StackTrace);
                    await Task.Delay(1000).ConfigureAwait(false);
                    if (i + 1 >= RetryCount)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 将 Token 写入文件。
        /// </summary>
        private async Task SaveToken()
        {
            await FileHelper.EncryptAndWriteFileAsync(AppFile.Token_data.GetFilePath(_localFolder),
                                                      JsonConvert.SerializeObject(MyToken));
        }

    }
}
