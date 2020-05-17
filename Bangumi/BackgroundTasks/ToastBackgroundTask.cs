using Bangumi.Api;
using Bangumi.Common;
using Bangumi.Helper;
using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Bangumi.BackgroundTasks
{
    public class ToastBackgroundTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral = null;
        private QueryString _queryString = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            _deferral = taskInstance.GetDeferral();

            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (details != null)
            {
                string arguments = details.Argument;
                _queryString = QueryString.Parse(arguments);

                switch (_queryString["action"])
                {
                    case "markEpWatched":
                        await MarkEpWatched(_queryString["episodeId"]);
                        break;
                    default:
                        break;
                }
            }
            _deferral.Complete();
        }

        /// <summary>
        /// Handles background task cancellation.
        /// </summary>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            ToastNotificationHelper.Toast("任务被取消", reason.ToString(), "重试", _queryString, ToastActivationType.Background);

            Debug.WriteLine("Background " + sender.Task.Name + " Cancel Requested...");
        }

        private async Task MarkEpWatched(string episodeId)
        {
            try
            {
                BangumiApi.Init(
                    Constants.ClientId,
                    Constants.ClientSecret,
                    Constants.RedirectUrl,
                    ApplicationData.Current.LocalFolder.Path,
                    ApplicationData.Current.LocalCacheFolder.Path,
                    EncryptionHelper.EncryptionAsync,
                    EncryptionHelper.DecryptionAsync);

                await BangumiApi.BgmApi.UpdateProgress(episodeId, Api.Models.EpStatusType.watched);
            }
            catch (Exception e)
            {
                ToastNotificationHelper.Toast("操作失败", e.Message, "重试", _queryString);
            }

        }

    }
}
