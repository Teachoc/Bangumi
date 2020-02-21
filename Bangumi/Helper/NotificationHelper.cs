using System;

namespace Bangumi.Helper
{
    public static class NotificationHelper
    {
        public static void Notify(string msg, NotifyType notifyType = NotifyType.Message)
        {
            string color;
            switch (notifyType)
            {
                case NotifyType.Message:
                    color = "#4caf50";
                    break;
                case NotifyType.Warn:
                    color = "#ffae22";
                    break;
                case NotifyType.Error:
                    color = "#f44336";
                    break;
                case NotifyType.Debug:
#if DEBUG
                    color = "#4caf50";
                    msg = DateTime.Now.ToLongTimeString() + ": " + msg;
                    break;
#else
                    return;
#endif
                default:
                    color = "#4caf50";
                    break;
            }
            MainPage.RootPage.NotifyControl.AddNotification(msg, color);
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
