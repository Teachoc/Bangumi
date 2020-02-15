using Bangumi.Api;
using Bangumi.Api.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace Bangumi.Helper
{
    internal static class OAuthHelper
    {
        /// <summary>
        /// 用户登录。
        /// </summary>
        /// <returns></returns>
        public static async Task Authorize()
        {
            try
            {
                string url = $"{BgmOAuth.OAuthHOST}/authorize?client_id={BgmOAuth.ClientId}&response_type=code";

                Uri startUri = new Uri(url);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                Uri endUri = new Uri($"{BgmOAuth.OAuthHOST}/{BgmOAuth.RedirectUrl}");

                //rootPage.NotifyUser("Navigating to: " + GoogleURL, NotifyType.StatusMessage);

                WebAuthenticationResult webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
                if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    await BangumiApi.BgmOAuth.GetToken(webAuthenticationResult.ResponseData.Replace($"{BgmOAuth.OAuthHOST}/{BgmOAuth.RedirectUrl}?code=", ""));
                }
                else if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    NotificationHelper.Notify("HTTP Error: " + webAuthenticationResult.ResponseErrorDetail.ToString());
                }
                else
                {
                    NotificationHelper.Notify("Error: " + webAuthenticationResult.ResponseStatus.ToString());
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify($"登录失败，请重试！\n{e.Message}", NotificationHelper.NotifyType.Error);
                Debug.WriteLine("换取Token失败。");
                Debug.WriteLine(e.StackTrace);
            }
        }


    }
}
