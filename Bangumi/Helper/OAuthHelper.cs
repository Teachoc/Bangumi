using Bangumi.Api;
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
                string url = $"{BangumiApi.OAuthBaseUrl}/authorize?client_id={BangumiApi.ClientId}&response_type=code";

                Uri startUri = new Uri(url);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                Uri endUri = new Uri($"{BangumiApi.OAuthBaseUrl}/{BangumiApi.RedirectUrl}");

                //rootPage.NotifyUser("Navigating to: " + GoogleURL, NotifyType.StatusMessage);

                WebAuthenticationResult webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
                if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    await BangumiApi.GetTokenAsync(webAuthenticationResult.ResponseData.Replace($"{BangumiApi.OAuthBaseUrl}/{BangumiApi.RedirectUrl}?code=", ""));
                }
                else if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    //OutputToken("HTTP Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseErrorDetail.ToString());
                }
                else
                {
                    //OutputToken("Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseStatus.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                //var msgDialog = new Windows.UI.Popups.MessageDialog("登录失败，请重试！\n" + e.Message) { Title = "登录失败！" };
                //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                //await msgDialog.ShowAsync();
                //rootPage.NotifyUser(Error.Message, NotifyType.ErrorMessage);
            }
        }


    }
}
