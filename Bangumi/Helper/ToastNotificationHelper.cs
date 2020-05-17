using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Bangumi.Helper
{
    public static class ToastNotificationHelper
    {
        public static void Toast(string title, string content,
            string buttonText = null, string buttonActionName = "",
            string propertyName = "", string propertyValue = "",
            string propertyName2 = "", string propertyValue2 = "",
            ToastActivationType toastActivationType = ToastActivationType.Foreground,
            bool silent = false)
        {
            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText{ Text = title },
                        new AdaptiveText{ Text = content },
                    },
                }
            };

            ToastActionsCustom actions = null;
            if (!string.IsNullOrEmpty(buttonText))
            {
                // Construct the actions for the toast (inputs and buttons)
                actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton(buttonText, new QueryString()
                        {
                            { "action", buttonActionName },
                            { propertyName, propertyValue },
                            { propertyName2, propertyValue2 },
                        }.ToString())
                        {
                            ActivationType = toastActivationType
                        }
                    }
                };
            }

            ToastAudio audio = new ToastAudio
            {
                Silent = silent
            };

            // Now we can construct the final toast content
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,
                Actions = actions,
                Audio = audio
            };

            // And create the toast notification
            var toast = new ToastNotification(toastContent.GetXml());

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public static void Toast(string title, string content,
            string buttonText, QueryString queries,
            ToastActivationType toastActivationType = ToastActivationType.Foreground)
        {
            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText{ Text = title },
                        new AdaptiveText{ Text = content },
                    },
                }
            };

            ToastActionsCustom actions = null;
            if (!string.IsNullOrEmpty(buttonText))
            {
                // Construct the actions for the toast (inputs and buttons)
                actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton(buttonText, queries.ToString())
                        {
                            ActivationType = toastActivationType
                        }
                    }
                };
            }

            // Now we can construct the final toast content
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,
                Actions = actions,
            };

            // And create the toast notification
            var toast = new ToastNotification(toastContent.GetXml());

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public static void ScheduledToast(DateTimeOffset deliveryTime,
            string title, string content,
            string buttonText = null, string buttonActionName = "",
            string propertyName = "", string propertyValue = "",
            string propertyName2 = "", string propertyValue2 = "",
            ToastActivationType toastActivationType = ToastActivationType.Foreground)
        {
            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText{ Text = title },
                        new AdaptiveText{ Text = content },
                    },
                }
            };

            ToastActionsCustom actions = null;
            if (!string.IsNullOrEmpty(buttonText))
            {
                // Construct the actions for the toast (inputs and buttons)
                actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton(buttonText, new QueryString()
                        {
                            { "action", buttonActionName },
                            { propertyName, propertyValue },
                            { propertyName2, propertyValue2 },
                        }.ToString())
                        {
                            ActivationType = toastActivationType
                        }
                    }
                };
            }

            // Now we can construct the final toast content
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,
                Actions = actions,
            };

            // And create the toast notification
            var toast = new ScheduledToastNotification(toastContent.GetXml(), deliveryTime);

            // And your scheduled toast to the schedule
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
        }

        public static void RemoveAllScheduledToasts()
        {
            // Create the toast notifier
            ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();

            // Get the list of scheduled toasts that haven't appeared yet
            IReadOnlyList<ScheduledToastNotification> scheduledToasts = notifier.GetScheduledToastNotifications();

            foreach (var item in scheduledToasts)
            {
                notifier.RemoveFromSchedule(item);
            }
        }
    }
}
