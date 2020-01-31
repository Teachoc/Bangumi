using Bangumi.Api;
using Bangumi.Api.Common;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Views;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bangumi.ViewModels
{
    public class TimeLineViewModel : ViewModelBase
    {
        public TimeLineViewModel()
        {
            IsLoading = false;
        }

        public ObservableCollection<Calendar> TimeLineCollection { get; private set; } = new ObservableCollection<Calendar>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                Set(ref _isLoading, value);
                HomePage.homePage.IsLoading = value;
                MainPage.RootPage.RefreshButton.IsEnabled = !value;
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
        /// 刷新时间表。
        /// </summary>
        public async Task LoadTimeLine()
        {
            try
            {
                IsLoading = true;
                await PopulateBangumiCalendarAsync();
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("获取时间表失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 更新条目的收藏状态
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="collectionStatus"></param>
        public async Task UpdateCollectionStatus(SubjectBase subject, CollectionStatusType collectionStatus)
        {
            if (subject != null)
            {
                IsUpdating = true;
                await BangumiFacade.UpdateCollectionStatusAsync(subject.Id.ToString(), collectionStatus);
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 显示时间表。
        /// </summary>
        /// <param name="bangumiTimeLine"></param>
        /// <returns></returns>
        private async Task PopulateBangumiCalendarAsync()
        {
            try
            {
                int day = GetDayOfWeek();
                if (TimeLineCollection.Count == 0)
                {
                    ProcessTimeLine(BangumiApi.BgmCache.Calendar());
                }

                await BangumiApi.BgmApi.Calendar()
                    .ContinueWith(async t =>
                    {
                        if (BangumiApi.BgmOAuth.IsLogin)
                        {
                            await BangumiApi.BgmApi.Status(t.Result.SelectMany(t => t.Items.Select(s => s.Id.ToString())));
                            foreach (var item in t.Result)
                            {
                                foreach (var subject in item.Items)
                                {
                                    subject.Status = BangumiApi.BgmCache.Status(subject.Id.ToString())?.Status?.Id;
                                }
                            }
                        }
                        if (!t.Result.SequenceEqualExT(TimeLineCollection.OrderBy(b => b.Weekday.Id).ToList()))
                        {
                            await DispatcherHelper.ExecuteOnUIThreadAsync(() => ProcessTimeLine(t.Result));
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap();


                // 处理时间表顺序
                void ProcessTimeLine(List<Calendar> timeLines)
                {
                    //清空原数据
                    TimeLineCollection.Clear();
                    foreach (var item in timeLines)
                    {
                        if (item.Weekday.Id < day)
                        {
                            TimeLineCollection.Add(item);
                        }
                        else
                        {
                            TimeLineCollection.Insert(TimeLineCollection.Count + 1 - day, item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("显示时间表失败。");
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// 获取当天星期几
        /// </summary>
        /// <returns></returns>
        private int GetDayOfWeek()
        {
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 3;
                case DayOfWeek.Thursday:
                    return 4;
                case DayOfWeek.Friday:
                    return 5;
                case DayOfWeek.Saturday:
                    return 6;
                case DayOfWeek.Sunday:
                    return 7;
                default:
                    return 1;
            }
        }


    }
}
