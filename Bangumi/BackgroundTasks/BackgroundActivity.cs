//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using Bangumi.Api;
using Bangumi.Api.Common;
using Bangumi.Api.Exceptions;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Bangumi.BackgroundTasks
{
    public class BackgroundActivity
    {
        BackgroundTaskCancellationReason _cancelReason = BackgroundTaskCancellationReason.Abort;
        volatile bool _cancelRequested = false;
        BackgroundTaskDeferral _deferral = null;
        uint _progress = 0;
        IBackgroundTaskInstance _taskInstance = null;

        /// <summary>
        /// The Run method is the entry point of a background task.
        /// </summary>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");

            // Query BackgroundWorkCost
            // Guidance: If BackgroundWorkCost is high, then perform only the minimum amount
            // of work in the background task and return immediately.
            var cost = BackgroundWorkCost.CurrentBackgroundWorkCost;

            // Associate a cancellation handler with the background task.
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            // Get the deferral object from the task instance, and take a reference to the taskInstance;
            _deferral = taskInstance.GetDeferral();
            _taskInstance = taskInstance;

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

                await BangumiApi.BgmOAuth.CheckToken();

                if (SettingHelper.EnableBangumiAirToast)
                {
                    // 初始化 BangumiData 对象
                    BangumiData.Init(Path.Combine(ApplicationData.Current.LocalFolder.Path, "bangumi-data"),
                                     SettingHelper.UseBiliApp);

                    if (!BangumiApi.BgmCache.IsUpdatedToday)
                    {
                        // 加载缓存，后面获取新数据后比较需要使用
                        var cachedWatchings = BangumiApi.BgmCache.Watching();

                        // 加载新的收视进度
                        var newWatching = await BangumiApi.BgmApi.Watching();
                        var subjectTasks = new List<Task<SubjectLarge>>();
                        var progressTasks = new List<Task<Progress>>();
                        // 新的收视进度与缓存的不同或未缓存的条目
                        var watchingsNotCached = BangumiApi.BgmCache.IsUpdatedToday ?
                                                 newWatching.Where(it => cachedWatchings.All(it2 => !it2.EqualsExT(it))).ToList() :
                                                 newWatching;
                        using (var semaphore = new SemaphoreSlim(10))
                        {
                            foreach (var item in watchingsNotCached)
                            {
                                await semaphore.WaitAsync();
                                subjectTasks.Add(BangumiApi.BgmApi.SubjectEp(item.SubjectId.ToString())
                                    .ContinueWith(t =>
                                    {
                                        semaphore.Release();
                                        return t.Result;
                                    }));
                                await semaphore.WaitAsync();
                                progressTasks.Add(BangumiApi.BgmApi.Progress(item.SubjectId.ToString())
                                    .ContinueWith(t =>
                                    {
                                        semaphore.Release();
                                        return t.Result;
                                    }));
                            }
                            await Task.WhenAll(subjectTasks);
                            await Task.WhenAll(progressTasks);
                        }
                        BangumiApi.BgmCache.IsUpdatedToday = true;
                        await BangumiApi.BgmCache.WriteToFile();
                    }

                    ToastNotificationHelper.RemoveAllScheduledToasts();
                    foreach (var item in CachedWatchProgress())
                    {
                        await item.ScheduleToast();
                    }
                }
            }
            catch (BgmUnauthorizedException e)
            {
                // 取消所有后台任务
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    cur.Value.Unregister(true);
                }
                ToastNotificationHelper.Toast("后台任务", "用户认证过期，后台任务已取消。");
                Debug.WriteLine(e.StackTrace);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
            finally
            {
                _deferral.Complete();
            }

            IEnumerable<WatchProgress> CachedWatchProgress()
            {
                foreach (var watching in BangumiApi.BgmCache.Watching())
                {
                    var subject = BangumiApi.BgmCache.Subject(watching.SubjectId.ToString());
                    var progress = BangumiApi.BgmCache.Progress(watching.SubjectId.ToString());

                    var item = WatchProgress.FromWatching(watching);
                    item.ProcessEpisode(subject);
                    item.ProcessProgress(progress);
                    if (subject == null || progress == null)
                    {
                        // 标记以重新加载
                        watching.Subject.Eps = -1;
                    }
                    yield return item;
                }
            }

        }

        /// <summary>
        /// Handles background task cancellation.
        /// </summary>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // Indicate that the background task is canceled.
            _cancelRequested = true;
            _cancelReason = reason;

            Debug.WriteLine("Background " + sender.Task.Name + " Cancel Requested...");
        }

        public static void Start(IBackgroundTaskInstance taskInstance)
        {
            // Use the taskInstance.Name and/or taskInstance.InstanceId to determine
            // what background activity to perform. In this sample, all of our
            // background activities are the same, so there is nothing to check.
            var activity = new BackgroundActivity();
            activity.Run(taskInstance);
        }
    }
}
