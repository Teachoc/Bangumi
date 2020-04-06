using Bangumi.Api;
using Bangumi.Api.Common;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.Controls;
using Bangumi.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bangumi.ViewModels
{
    public class CalendarViewModel : ViewModelBase
    {
        public CalendarViewModel()
        {
            IsLoading = false;
        }

        public ObservableCollection<Calendar> CalendarCollection { get; private set; } = new ObservableCollection<Calendar>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                Set(ref _isLoading, value);
                MainPage.RootPage.PageStatusChanged();
            }
        }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                Set(ref _isUpdating, value);
            }
        }

        /// <summary>
        /// 更新条目的收藏状态
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="collectionStatus"></param>
        public async Task UpdateCollectionStatus(SubjectForCalendar subject, CollectionStatusType collectionStatus)
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (subject != null)
            {
                IsUpdating = true;
                try
                {
                    var status = await BangumiApi.BgmApi.UpdateStatus(subject.Id.ToString(), collectionStatus);
                    subject.Status = status.Status.Id;
                    NotificationHelper.Notify($"标记 {Converters.StringOneOrTwo(subject.NameCn, subject.Name)} {status.Status.Id.GetDesc(subject.Type)} 成功！");
                }
                catch (Exception e)
                {
                    NotificationHelper.Notify($"标记 {Converters.StringOneOrTwo(subject.NameCn, subject.Name)} {collectionStatus.GetDesc(subject.Type)} 失败！\n" + e.Message,
                                              NotifyType.Error);
                }
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 显示时间表。
        /// </summary>
        public async Task PopulateCalendarAsync()
        {
            if (NetworkHelper.IsOffline)
            {
                return;
            }
            try
            {
                IsLoading = true;
                var calendar = await BangumiApi.BgmApi.Calendar();
                if (BangumiApi.BgmOAuth.IsLogin)
                {
                    await BangumiApi.BgmApi.Status(calendar.SelectMany(t => t.Items.Select(s => s.Id.ToString())));
                }
                ProgressStatus(calendar);
                if (!calendar.SequenceEqualExT(CalendarCollection.OrderBy(b => b.Weekday.Id).ToList()))
                {
                    ProcessTimeLine(calendar);
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("获取时间表失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotifyType.Error);
                Debug.WriteLine(e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void PopulateCalendarFromCache()
        {
            var cache = BangumiApi.BgmCache.Calendar();
            ProgressStatus(cache);
            if (!cache.SequenceEqualExT(CalendarCollection.OrderBy(b => b.Weekday.Id).ToList()))
            {
                ProcessTimeLine(cache);
            }
        }

        /// <summary>
        /// 处理条目收藏状态
        /// </summary>
        private void ProgressStatus(List<Calendar> calendars)
        {
            foreach (var item in calendars)
            {
                foreach (var subject in item.Items)
                {
                    subject.Status = BangumiApi.BgmCache.Status(subject.Id.ToString())?.Status?.Id;
                }
            }
        }

        /// <summary>
        /// 处理时间表顺序
        /// </summary>
        private void ProcessTimeLine(List<Calendar> timeLines)
        {
            int day = GetDayOfWeek();
            //清空原数据
            CalendarCollection.Clear();
            foreach (var item in timeLines)
            {
                if (item.Weekday.Id < day)
                {
                    CalendarCollection.Add(item);
                }
                else
                {
                    CalendarCollection.Insert(CalendarCollection.Count + 1 - day, item);
                }
            }
        }

        /// <summary>
        /// 获取当天星期几
        /// </summary>
        private int GetDayOfWeek()
        {
            return DateTime.Today.DayOfWeek switch
            {
                DayOfWeek.Monday => 1,
                DayOfWeek.Tuesday => 2,
                DayOfWeek.Wednesday => 3,
                DayOfWeek.Thursday => 4,
                DayOfWeek.Friday => 5,
                DayOfWeek.Saturday => 6,
                DayOfWeek.Sunday => 7,
                _ => throw new Exception("No such day.")
            };
        }


    }
}
