using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Timers;
using Microsoft.Toolkit.Uwp.Helpers;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Bangumi.Controls
{
    public sealed partial class InAppNotification : UserControl
    {
        private ObservableCollection<NotifyMessage> Notifies = new ObservableCollection<NotifyMessage>();
        private TimeSpan _delay = TimeSpan.FromSeconds(3);
        private DateTime _lastMessage;
        private Timer _timer = new Timer();


        public InAppNotification()
        {
            this.InitializeComponent();
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = false;
        }

        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => Notifies.RemoveAt(0));
            if (Notifies.Count > 0)
            {
                _timer.Interval = Notifies[0].ExpiresIn.TotalMilliseconds;
                _timer.Start();
            }
        }

        public void AddNotification(string msg, string color)
        {
            if (!_timer.Enabled)
            {
                _timer.Interval = _delay.TotalMilliseconds;
                _timer.Start();
            }
            var now = DateTime.Now;
            Notifies.Add(new NotifyMessage
            {
                Color = color,
                Message = msg,
                ExpiresIn = now - _lastMessage
            });
            _lastMessage = now;
        }
    }

    public class NotifyMessage
    {
        public string Color { get; set; }
        public string Message { get; set; }
        public TimeSpan ExpiresIn { get; set; }
    }
}
