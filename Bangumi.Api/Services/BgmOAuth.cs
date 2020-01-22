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

        private readonly string _localFolder;
        private static IBgmCache _bgmCache;

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
                client.Settings.OnErrorAsync = async call =>
                {
                    if (call.HttpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("刷新Token。");
                        await RefreshToken();
                    }
                    else if (call.HttpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
                    {
                        if ((await call.HttpResponseMessage.Content.ReadAsStringAsync()).Contains("Invalid refresh token"))
                        {
                            throw new BgmUnauthorizedException();
                        }
                    }
                    if (call.Exception is FlurlHttpTimeoutException)
                    {
                        throw new BgmTimeoutException();
                    }
                };
            });
        }

        public async Task GetToken(string code)
        {
            // 重试最多三次
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Debug.WriteLine($"第{i + 1}次尝试获取Token。");
                    await $"{OAuthHOST}/access_token"
                        .PostUrlEncodedAsync(new
                        {
                            grant_type = "authorization_code",
                            client_id = ClientId,
                            client_secret = ClientSecret,
                            code,
                            redirect_uri = RedirectUrl
                        })
                        .ReceiveJson<AccessToken>()
                        .ContinueWith(async t =>
                        {
                            MyToken = t.Result;
                            MyToken.Expires = (int)DateTime.Now.AddSeconds(t.Result.ExpiresIn).ToJsTick();
                            await SaveToken();
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"第{i + 1}次尝试获取Token失败。");
                    Debug.WriteLine(e.StackTrace);
                    await Task.Delay(1000);
                }
            }
        }

        public async Task<bool> CheckToken()
        {
            if (MyToken == null) return false;
            Debug.WriteLine("检查 Token 有效期。");
            await $"{OAuthHOST}/token_status"
                .PostStringAsync(string.Empty)
                .ReceiveJson<AccessToken>()
                .ContinueWith(async t =>
                {
                    // 若token对应的用户ID不同，
                    // 获取1天后的时间戳，离过期不足1天时或过期后，
                    // 更新 access_token
                    if (t.Result.UserId != MyToken.UserId || t.Result.Expires < DateTime.Now.AddDays(1).ToJsTick())
                        await RefreshToken();
                });
            return true;
        }

        public void DeleteUserFiles()
        {
            // 删除用户认证文件
            MyToken = null;
            FileHelper.DeleteFile(AppFile.Token_data.GetFilePath(_localFolder));
            // 清空缓存
            _bgmCache.Delete();
        }

        public async Task RefreshToken()
        {
            if (MyToken == null) return;
            await $"{OAuthHOST}/access_token"
                .PostUrlEncodedAsync(new
                {
                    grant_type = "refresh_token",
                    client_id = ClientId,
                    client_secret = ClientSecret,
                    refresh_token = MyToken.RefreshToken,
                    redirect_uri = RedirectUrl
                })
                .ReceiveJson<AccessToken>()
                .ContinueWith(async t =>
                {
                    MyToken = t.Result;
                    MyToken.Expires = (int)DateTime.Now.AddSeconds(t.Result.ExpiresIn).ToJsTick();
                    await SaveToken();
                });
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
