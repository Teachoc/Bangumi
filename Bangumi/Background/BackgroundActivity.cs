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
using Bangumi.Common;
using Bangumi.Helper;
using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Bangumi.Background
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
                    null,
                    EncryptionHelper.EncryptionAsync,
                    EncryptionHelper.DecryptionAsync);

                await BangumiApi.BgmOAuth.CheckToken();
            }
            catch (BgmUnauthorizedException e)
            {
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    if (cur.Value.Name == "RefreshTokenTask")
                    {
                        cur.Value.Unregister(true);
                    }
                }
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
