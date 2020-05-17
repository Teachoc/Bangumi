using Bangumi.Api;
using Bangumi.Api.Common;
using Bangumi.Api.Exceptions;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Controls;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bangumi.ViewModels
{
    public class ProgressViewModel : ViewModelBase
    {
        public ProgressViewModel() => IsLoading = false;

        public ObservableCollection<WatchProgress> WatchingCollection { get; private set; } = new ObservableCollection<WatchProgress>();

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


        #region 公开方法
        /// <summary>
        /// 处理并显示用户收视进度列表。
        /// </summary>
        public async Task PopulateWatchingListAsync()
        {
            if (NetworkHelper.IsOffline)
            {
                return;
            }
            try
            {
                IsLoading = true;
                // 检查 token
                try
                {
                    await BangumiApi.BgmOAuth.CheckToken();
                }
                catch (BgmUnauthorizedException)
                {
                    // 授权过期，返回登录界面
                    MainPage.RootPage.Frame.Navigate(typeof(LoginPage), Constants.UnauthorizedImgUri);
                    return;
                }
                catch (Exception)
                {
                    return;
                }
                // 加载缓存，后面获取新数据后比较需要使用
                var cachedWatchings = BangumiApi.BgmCache.Watching();
                var cachedWatchStatus = CachedWatchProgress().ToList();

                // 加载新的收视进度
                var newWatchStatus = new List<WatchProgress>();
                var newWatching = await BangumiApi.BgmApi.Watching();
                var subjectTasks = new List<Task<SubjectLarge>>();
                var progressTasks = new List<Task<Progress>>();
                SubjectLarge[] newSubjects = null;
                Progress[] newProgresses = null;
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
                    newSubjects = await Task.WhenAll(subjectTasks);
                    newProgresses = await Task.WhenAll(progressTasks);
                }
                foreach (var watching in newWatching)
                {
                    WatchProgress item;
                    if (watchingsNotCached.Any(it => it.SubjectId == watching.SubjectId))
                    {
                        item = WatchProgress.FromWatching(watching);
                        item.ProcessEpisode(newSubjects.FirstOrDefault(it => it.Id == item.SubjectId));
                        item.ProcessProgress(newProgresses.FirstOrDefault(it => it?.SubjectId == item.SubjectId));
                    }
                    else
                    {
                        item = cachedWatchStatus.Find(it => it.SubjectId == watching.SubjectId);
                    }
                    newWatchStatus.Add(item);
                }
                BangumiApi.BgmCache.IsUpdatedToday = true;
                CollectionHelper.Differ(WatchingCollection, SortWatchProgress(newWatchStatus).ToList());

                if (SettingHelper.EnableBangumiAirToast)
                {
                    ToastNotificationHelper.RemoveAllScheduledToasts();
                    foreach (var item in CachedWatchProgress())
                    {
                        var first = item.Eps?.FirstOrDefault(ep => ep.Type == EpisodeType.本篇)?.AirDate;
                        var last = item.Eps?.LastOrDefault(ep => ep.Type == EpisodeType.本篇 && !Regex.IsMatch(ep.Status, "(NA)"))?.AirDate;
                        if (SettingHelper.UseBangumiDataAirTime &&
                            first != null && first != DateTime.MinValue && last != null && last != DateTime.MinValue &&
                            BangumiData.GetAirTimeByBangumiId(item.SubjectId.ToString())?.ToLocalTime() is DateTimeOffset date)
                        {
                            var airTime = date.AddTicks(last.Value.Ticks).AddTicks(-first.Value.Ticks);
                            if (airTime > DateTimeOffset.Now)
                            {
                                if (!SettingHelper.UseActionCenterMode)
                                {
                                    ToastNotificationHelper.ScheduledToast(airTime, Converters.StringOneOrTwo(item.NameCn, item.Name),
                                        $"更新到：{item.NextEpDesc}", "查看", "viewSubject", "subjectId", item.SubjectId.ToString());
                                }
                                else
                                {
                                    var sites = await BangumiData.GetAirSitesByBangumiIdAsync(item.SubjectId.ToString());
                                    var site = sites.FirstOrDefault();
                                    ToastNotificationHelper.ScheduledToast(airTime, Converters.StringOneOrTwo(item.NameCn, item.Name),
                                        $"更新到：{item.NextEpDesc}", $"前往{site.SiteName}播放", "gotoPlaySite", "url", site.Url, "episode", JsonConvert.SerializeObject(item.NextEp));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("获取收视进度失败！\n" + e.Message,
                                          NotifyType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void PopulateWatchingListFromCache()
        {
            CollectionHelper.Differ(WatchingCollection, SortWatchProgress(CachedWatchProgress()).ToList());
        }

        /// <summary>
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        public async Task EditCollectionStatus(WatchProgress item)
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            var subjectStatus = BangumiApi.BgmApi.Status(item.SubjectId.ToString());
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog(
                item.Type, subjectStatus)
            {
                Title = Converters.StringOneOrTwo(item.NameCn, item.Name)
            };
            MainPage.RootPage.HasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync() &&
                collectionEditContentDialog.CollectionStatus != null)
            {
                item.IsUpdating = true;
                try
                {
                    var collectionStatusE = await BangumiApi.BgmApi.UpdateStatus(item.SubjectId.ToString(),
                        collectionEditContentDialog.CollectionStatus.Value,
                        collectionEditContentDialog.Comment,
                        collectionEditContentDialog.Rate.ToString(),
                        collectionEditContentDialog.Privacy ? "1" : "0");
                    if (collectionStatusE.Status.Id != CollectionStatusType.Do)
                    {
                        // 若修改后状态不是在看，则从进度页面删除
                        WatchingCollection.Remove(item);
                    }
                    NotificationHelper.Notify($"标记 {collectionEditContentDialog.Title} {collectionStatusE.Status.Id.GetDesc(item.Type)} 成功！");
                }
                catch (Exception e)
                {
                    NotificationHelper.Notify($"标记 {collectionEditContentDialog.Title} {collectionEditContentDialog.CollectionStatus?.GetDesc(item.Type)} 失败！\n" + e.Message,
                                              NotifyType.Error);
                }
                item.IsUpdating = false;
            }
            MainPage.RootPage.HasDialog = false;
        }

        /// <summary>
        /// 更新下一章章节状态为已看
        /// </summary>
        public async Task MarkNextEpWatched(WatchProgress item)
        {
            if (item == null || item.NextEp == null)
            {
                return;
            }
            var color = item.EpColor;
            await item.MarkNextEpWatched();
            if (SettingHelper.OrderByAirTime && color != item.EpColor)
            {
                var oldIndex = WatchingCollection.IndexOf(item);
                var newIndex = SortWatchProgress(WatchingCollection).ToList().IndexOf(item);
                if (newIndex != oldIndex)
                {
                    WatchingCollection.Remove(item);
                    WatchingCollection.Insert(newIndex, item);
                }
            }
            // 若设置启用且看完则弹窗提示修改收藏状态及评价
            if (SettingHelper.SubjectComplete && item.NextEp == null)
            {
                await EditCollectionStatus(item);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 缓存的收视进度
        /// </summary>
        private IEnumerable<WatchProgress> CachedWatchProgress()
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

        /// <summary>
        /// 对条目进行排序
        /// </summary>
        private IEnumerable<WatchProgress> SortWatchProgress(IEnumerable<WatchProgress> watchingStatuses)
        {
            if (SettingHelper.OrderByAirTime)
            {
                return watchingStatuses.OrderBy(p => p.EpColor)
                    .ThenBy(p => p.WatchedEpsCount == 0)
                    //.ThenBy(p => p.AirEpsCount - p.WatchedEpsCount) //尝试不依靠已放送章节数与已观看章节数之差排序
                    .ThenBy(p => p.Eps?.LastOrDefault(ep => ep.Type == EpisodeType.本篇 && !Regex.IsMatch(ep.Status, "(NA)"))?.AirDate)
                    .ThenBy(p => p.AirTime);
            }
            return watchingStatuses.OrderByDescending(w => w.LastTouch);
        }

        #endregion

    }


    public class WatchProgress : ViewModelBase
    {
        public string Name { get; set; }
        public string NameCn { get; set; }
        public int SubjectId { get; set; }
        public long LastTouch { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public SubjectType Type { get; set; }
        public string AirTime { get; set; }
        /// <summary>
        /// 正片数量
        /// </summary>
        public int EpsCount => Eps?.Count(ep => ep.Type == EpisodeType.本篇) ?? -1;
        public List<EpisodeForSort> Eps { get; set; }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                Set(ref _isUpdating, value);
                if (!value)
                {
                    OnPropertyChanged(nameof(AirEpsCount));
                    OnPropertyChanged(nameof(WatchedEpsCount));
                    OnPropertyChanged(nameof(WatchedAndAirEpsCountDesc));
                    OnPropertyChanged(nameof(NextEp));
                    OnPropertyChanged(nameof(NextEpDesc));
                    OnPropertyChanged(nameof(EpColor));
                }
            }
        }

        public int AirEpsCount => Eps?.Count(ep => ep.Type == EpisodeType.本篇 && Regex.IsMatch(ep.Status, "(Air|Today)")) ?? -1;
        public int WatchedEpsCount => Eps?.Count(ep => ep.Type == EpisodeType.本篇 && ep.EpStatus != EpStatusType.remove) ?? -1;
        public string WatchedAndAirEpsCountDesc
        {
            get
            {
                string air;
                if (EpsCount == -1)
                {
                    air = "无章节";
                }
                else if (AirEpsCount == 0)
                {
                    air = "尚未放送";
                }
                else if (AirEpsCount < EpsCount)
                {
                    air = "更新到" + AirEpsCount + "话";
                }
                else
                {
                    air = "全" + AirEpsCount + "话";
                }
                string watch;
                if (WatchedEpsCount == -1)
                {
                    return air;
                }
                else if (WatchedEpsCount == 0)
                {
                    watch = "尚未观看";
                }
                else
                {
                    watch = "看到" + WatchedEpsCount + "话";
                }
                return $"{watch} / {air}";
            }
        }
        public EpisodeForSort NextEp => Eps?.FirstOrDefault(ep => ep.Type == EpisodeType.本篇 && ep.EpStatus == EpStatusType.remove);
        public string NextEpDesc => $"EP.{NextEp?.Sort} {Converters.StringOneOrTwo(NextEp?.NameCn, NextEp?.Name)}";
        public string EpColor => (NextEp == null || NextEp.Status == "NA") ? "Gray" : "#d26585";

        public static WatchProgress FromWatching(Watching w) => new WatchProgress
        {
            Name = w.Subject.Name,
            NameCn = w.Subject.NameCn,
            Image = w.Subject.Images.Common,
            SubjectId = w.SubjectId,
            Url = w.Subject.Url,
            LastTouch = w.LastTouch, // 该条目上次修改时间
            AirTime = w.Subject.AirDate,
            Type = w.Subject.Type,
            IsUpdating = false,
        };

        public void ProcessEpisode(SubjectLarge subject)
        {
            if (subject?.Eps != null)
            {
                Eps = new List<EpisodeForSort>();
                foreach (var ep in subject.Eps)
                {
                    Eps.Add(EpisodeForSort.FromEpisode(ep));
                }
            }
            var first = Eps?.FirstOrDefault(ep => ep.Type == EpisodeType.本篇)?.AirDate;
            var last = Eps?.LastOrDefault(ep => ep.Type == EpisodeType.本篇 && !Regex.IsMatch(ep.Status, "(NA)"))?.AirDate;
            if (SettingHelper.UseBangumiDataAirTime &&
                BangumiData.GetAirTimeByBangumiId(subject.Id.ToString())?.ToLocalTime() is DateTimeOffset date)
            {
                var dateFormat = "yyyy-MM-dd HH:mm";
                if (first != null && first != DateTime.MinValue && last != null && last != DateTime.MinValue)
                {
                    AirTime = date.AddTicks(last.Value.Ticks).AddTicks(-first.Value.Ticks).ToString(dateFormat);
                }
                else
                {
                    AirTime = date.ToString(dateFormat);
                }
            }
            else if (DateTime.TryParse(AirTime, out var airDate))
            {
                var dateFormat = "yyyy-MM-dd";
                if (first != null && first != DateTime.MinValue && last != null && last != DateTime.MinValue)
                {
                    AirTime = airDate.AddTicks(last.Value.Ticks).AddTicks(-first.Value.Ticks).ToString(dateFormat);
                }
                else
                {
                    AirTime = airDate.ToString(dateFormat);
                }
            }
        }

        public void ProcessProgress(Progress progress)
        {
            if (progress?.Eps != null)
            {
                // 填充用户观看状态
                foreach (var ep in Eps)
                {
                    var prog = progress.Eps.Where(p => p.Id == ep.Id).FirstOrDefault();
                    ep.EpStatus = prog?.Status?.Id ?? EpStatusType.remove;
                }
            }
        }

        /// <summary>
        /// 标记下一话为看过
        /// </summary>
        public async Task MarkNextEpWatched()
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (NextEp == null)
            {
                return;
            }
            var next = NextEp;
            try
            {
                IsUpdating = true;
                if (await BangumiApi.BgmApi.UpdateProgress(next.Id.ToString(), EpStatusType.watched))
                {
                    next.EpStatus = EpStatusType.watched;
                    LastTouch = DateTime.Now.ToJsTick();
                    NotificationHelper.Notify($"标记 ep.{next.Sort} {Converters.StringOneOrTwo(next.NameCn, next.Name)} {EpStatusType.watched.GetCnName()}成功");
                }
                else
                {
                    NotificationHelper.Notify($"标记 ep.{next.Sort} {Converters.StringOneOrTwo(next.NameCn, next.Name)} {EpStatusType.watched.GetCnName()}失败，请重试！",
                                              NotifyType.Warn);
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify($"标记 ep.{next.Sort} {Converters.StringOneOrTwo(next.NameCn, next.Name)} {EpStatusType.watched.GetCnName()}失败！\n错误信息：{e.Message}",
                                          NotifyType.Error);
            }
            finally
            {
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 标记下一话为看过，静默（后台任务使用）
        /// </summary>
        public async Task<(bool, string)> MarkNextEpWatchedSilently()
        {
            if (NetworkHelper.IsOffline)
            {
                return (false, "无网络连接！");
            }
            var next = NextEp;
            try
            {
                if (await BangumiApi.BgmApi.UpdateProgress(next.Id.ToString(), EpStatusType.watched))
                {
                    next.EpStatus = EpStatusType.watched;
                    LastTouch = DateTime.Now.ToJsTick();
                    return (true, $"标记 ep.{next.Sort} {Converters.StringOneOrTwo(next.NameCn, next.Name)} {EpStatusType.watched.GetCnName()}成功");
                }
                else
                {
                    return (false, $"标记 ep.{next.Sort} {Converters.StringOneOrTwo(next.NameCn, next.Name)} {EpStatusType.watched.GetCnName()}失败，请重试！");
                }
            }
            catch (Exception e)
            {
                return (false, $"标记 ep.{next.Sort} {Converters.StringOneOrTwo(next.NameCn, next.Name)} {EpStatusType.watched.GetCnName()}失败！\n错误信息：{e.Message}");
            }
        }


        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            WatchProgress w = (WatchProgress)obj;
            return SubjectId == w.SubjectId &&
                   LastTouch == w.LastTouch &&
                   Type == w.Type &&
                   AirTime == w.AirTime &&
                   IsUpdating == w.IsUpdating &&
                   Name.EqualsExT(w.Name) &&
                   NameCn.EqualsExT(w.NameCn) &&
                   Url.EqualsExT(w.Url) &&
                   Image.EqualsExT(w.Image) &&
                   Eps.SequenceEqualExT(w.Eps);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class EpisodeForSort : EpisodeWithEpStatus
    {
        public new DateTime AirDate { get; set; }

        public new static EpisodeForSort FromEpisode(Episode ep) => new EpisodeForSort
        {
            Id = ep.Id,
            Url = ep.Url,
            Type = ep.Type,
            Sort = ep.Sort,
            Name = ep.Name,
            NameCn = ep.NameCn,
            Duration = ep.Duration,
            AirDate = DateTime.TryParse(ep.AirDate, out var d) ? d : d,
            Comment = ep.Comment,
            Desc = ep.Desc,
            Status = ep.Status,
            EpStatus = EpStatusType.remove
        };
    }
}
