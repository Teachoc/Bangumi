using Bangumi.Controls;
using System;

namespace Bangumi.Helper
{
    public static class NotificationHelper
    {
        public static void Notify(string msg, NotifyType notifyType = NotifyType.Message)
        {
            MainPage.RootPage.NotifyControl.AddNotification(msg, notifyType);
        }

    }
}
