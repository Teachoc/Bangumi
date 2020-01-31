using System;

namespace Bangumi.Helper
{
    public static class NotificationHelper
    {
        public static void Notify(string msg, NotifyType notifyType = NotifyType.Message)
        {
            Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification notification;
            int duration;
            bool dismissButton;
            switch (notifyType)
            {
                case NotifyType.Message:
                    notification = MainPage.RootPage.ToastInAppNotification;
                    duration = 1500;
                    dismissButton = false;
                    break;
                case NotifyType.Warn:
                    notification = MainPage.RootPage.ErrorInAppNotification;
                    duration = 1500;
                    dismissButton = false;
                    break;
                case NotifyType.Error:
                    notification = MainPage.RootPage.ErrorInAppNotification;
                    duration = 3000;
                    dismissButton = false;
                    break;
                case NotifyType.Debug:
#if DEBUG
                    MainPage.RootPage.ToastInAppNotification.Dismiss();
                    notification = MainPage.RootPage.ToastInAppNotification;
                    msg = DateTime.Now.ToLongTimeString() + ": " + msg;
                    duration = 0;
                    dismissButton = true;
                    break;
#else
                    return;
#endif
                default:
                    notification = MainPage.RootPage.ToastInAppNotification;
                    duration = 2000;
                    dismissButton = false;
                    break;
            }
            notification.ShowDismissButton = dismissButton;
            notification.Show(msg, duration);
        }

        public enum NotifyType
        {
            Message,
            Warn,
            Error,
            Debug,
        }
    }
}
