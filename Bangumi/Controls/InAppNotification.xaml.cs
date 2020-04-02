using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Xaml.Controls;

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

        public async Task AddNotification(string msg, NotifyType notifyType = NotifyType.Message)
        {
            if (!_timer.Enabled)
            {
                _timer.Interval = _delay.TotalMilliseconds;
                _timer.Start();
            }
            var now = DateTime.Now;
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                Notifies.Add(new NotifyMessage
                {
                    Color = GetColor(notifyType),
                    Message = msg,
                    ExpiresIn = now - _lastMessage
                });
            });
            _lastMessage = now;

            string GetColor(NotifyType notifyType)
            {
                return notifyType switch
                {
                    NotifyType.Message => "#4caf50",
                    NotifyType.Warn => "#ffae22",
                    NotifyType.Error => "#f44336",
                    _ => "#4caf50",
                };
            }
        }
    }

    public class NotifyMessage
    {
        public string Color { get; set; }
        public string Message { get; set; }
        public TimeSpan ExpiresIn { get; set; }
    }
    public enum NotifyType
    {
        Message,
        Warn,
        Error,
    }

}
