using Bangumi.Api;
using Bangumi.Api.Common;
using Bangumi.Api.Exceptions;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Data;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Views;
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

        public ObservableCollection<WatchStatus> WatchingCollection { get; private set; } = new ObservableCollection<WatchStatus>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                Set(ref _isLoading, value);
                HomePage.homePage.IsLoading = value;
                MainPage.RootPage.RefreshButton.IsEnabled = !value;
            }
        }


        #region 公开方法
        /// <summary>
        /// 处理并显示用户收视进度列表。
        /// </summary>
        /// <param name="watchingCollection"></param>
        /// <returns></returns>
        public async Task PopulateWatchingListAsync()
        {
            try
            {
                IsLoading = true;
                // 加载缓存，后面获取新数据后比较需要使用
                var cachedWatchings = BangumiApi.BgmCache.Watching();
                var cachedWatchStatus = CachedWatchStatus().ToList();

                // 加载新的收视进度
                var newWatchStatus = new List<WatchStatus>();
                await BangumiApi.BgmApi.Watching()
                    .ContinueWith(async t =>
                    {
                        var subjectTasks = new List<Task<SubjectLarge>>();
                        var progressTasks = new List<Task<Progress>>();
                        SubjectLarge[] newSubjects = null;
                        Progress[] newProgresses = null;
                        // 新的收视进度与缓存的不同或未缓存的条目
                        var watchingsNotCached = BangumiApi.BgmCache.IsUpdatedToday ?
                                                 t.Result.Where(it => cachedWatchings.All(it2 => !it2.EqualsExT(it))).ToList() :
                                                 t.Result;
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
                            newSubjects = await Task.WhenAll(subjectTasks).ConfigureAwait(false);
                            newProgresses = await Task.WhenAll(progressTasks).ConfigureAwait(false);
                        }
                        foreach (var watching in t.Result)
                        {
                            WatchStatus item;
                            if (watchingsNotCached.Any(it => it.SubjectId == watching.SubjectId))
                            {
                                item = ProcessWatching(watching);
                                ProcessSubject(item, newSubjects.FirstOrDefault(it => it.Id == item.SubjectId));
                                ProcessProgress(item, newProgresses.FirstOrDefault(it => it?.SubjectId == item.SubjectId));
                            }
                            else
                            {
                                item = cachedWatchStatus.Find(it => it.SubjectId == watching.SubjectId);
                            }
                            newWatchStatus.Add(item);
                        }
                        BangumiApi.BgmCache.IsUpdatedToday = true;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion)
                    .Unwrap();
                DiffListToObservableCollection(WatchingCollection, CollectionSorting(newWatchStatus));
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("获取收视进度失败！\n" + e.Message + e.StackTrace,
                                          NotificationHelper.NotifyType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void PopulateWatchingListFromCache()
        {
            DiffListToObservableCollection(WatchingCollection, CollectionSorting(CachedWatchStatus()));
        }

        /// <summary>
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        /// <param name="status"></param>
        public Task EditCollectionStatus(WatchStatus status)
        {
            if (status == null)
            {
                throw new ArgumentNullException(nameof(status));
            }
            return EditCollectionStatusInternal(status);
        }

        private async Task EditCollectionStatusInternal(WatchStatus status)
        {
            var subjectStatus = BangumiApi.BgmApi.Status(status.SubjectId.ToString());
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog(
                status.Type, subjectStatus)
            {
                Title = status.NameCn,
            };
            MainPage.RootPage.HasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync() &&
                collectionEditContentDialog.CollectionStatus != null)
            {
                status.IsUpdating = true;
                if (await BangumiFacade.UpdateCollectionStatusAsync(status.SubjectId.ToString(),
                                                                    collectionEditContentDialog.CollectionStatus.Value,
                                                                    collectionEditContentDialog.Comment,
                                                                    collectionEditContentDialog.Rate.ToString(),
                                                                    collectionEditContentDialog.Privacy ? "1" : "0")
                    && collectionEditContentDialog.CollectionStatus != CollectionStatusType.Do)
                {
                    // 若修改后状态不是在看，则从进度页面删除
                    WatchingCollection.Remove(status);
                }
                status.IsUpdating = false;
            }
            MainPage.RootPage.HasDialog = false;
        }

        /// <summary>
        /// 更新下一章章节状态为已看
        /// </summary>
        /// <param name="item"></param>
        public async Task UpdateNextEpStatus(WatchStatus item)
        {
            if (item?.Eps == null || item.NextEp == -1)
            {
                return;
            }
            var markEp = item.Eps.FirstOrDefault(ep => Regex.IsMatch(ep.Status, "(Air|Today|NA)") && ep.Sort == item.NextEp);
            if (markEp == null)
            {
                return;
            }
            item.IsUpdating = true;
            if (await BangumiFacade.UpdateProgressAsync(markEp.Id.ToString(), EpStatusType.watched))
            {
                markEp.Status = "看过";
                item.WatchedEps++;
                item.NextEp = item.Eps.Where(ep => ep.Type == 0 && Regex.IsMatch(ep.Status, "(Air|Today)")).OrderBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                              item.Eps.Where(ep => ep.Type == 0 && Regex.IsMatch(ep.Status, "(NA)")).OrderBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                              -1;
                // 若未看到最新一集，则使用粉色，否则使用灰色
                if (item.Eps.FirstOrDefault(ep => ep.Type == 0 && Regex.IsMatch(ep.Status, "(Air|Today)")) != null)
                {
                    item.EpColor = "#d26585";
                }
                else
                {
                    // 将已看到最新剧集的条目排到最后，且设为灰色
                    if (WatchingCollection.IndexOf(item) != WatchingCollection.Count - 1)
                    {
                        WatchingCollection.Remove(item);
                        WatchingCollection.Add(item);
                    }
                    item.EpColor = "Gray";

                    // 若设置启用且看完则弹窗提示修改收藏状态及评价
                    if (SettingHelper.SubjectComplete && item.Eps.FirstOrDefault(ep => ep.Type == 0 && Regex.IsMatch(ep.Status, "(Air|Today|NA)")) == null)
                    {
                        await EditCollectionStatus(item);
                    }
                }
                item.LastTouch = DateTime.Now.ToJsTick();
            }
            item.IsUpdating = false;
        }

        #endregion

        #region 私有方法

        #region 进度
        /// <summary>
        /// 缓存的收视进度
        /// </summary>
        /// <returns></returns>
        private IEnumerable<WatchStatus> CachedWatchStatus()
        {
            foreach (var watching in BangumiApi.BgmCache.Watching())
            {
                var subject = BangumiApi.BgmCache.Subject(watching.SubjectId.ToString());
                var progress = BangumiApi.BgmCache.Progress(watching.SubjectId.ToString());

                var item = ProcessWatching(watching);
                ProcessSubject(item, subject);
                ProcessProgress(item, progress);
                if (subject == null || progress == null)
                {
                    watching.LastTouch = 0;
                }
                yield return item;
            }
        }

        /// <summary>
        /// 将 Watching 转换为界面显示用的 WatchStatus
        /// </summary>
        private WatchStatus ProcessWatching(Watching w)
        {
            return new WatchStatus
            {
                Name = w.Subject.Name,
                NameCn = w.Subject.NameCn,
                Image = w.Subject.Images.Common,
                SubjectId = w.SubjectId,
                Url = w.Subject.Url,
                EpColor = "Gray",
                LastTouch = w.LastTouch, // 该条目上次修改时间
                WatchedEps = w.EpStatus,
                UpdatedEps = w.Subject.EpsCount,
                AirTime = SettingHelper.UseBangumiDataAirTime
                    ? BangumiData.GetAirTimeByBangumiId(w.SubjectId.ToString())?.ToString("yyyy-MM-dd HH:mm")
                    ?? w.Subject.AirDate
                    : w.Subject.AirDate,
                Type = w.Subject.Type,
                IsUpdating = false,
            };
        }

        /// <summary>
        /// 处理条目章节
        /// </summary>
        /// <param name="item">显示条目</param>
        /// <param name="subject">条目章节</param>
        /// <returns></returns>
        private void ProcessSubject(WatchStatus item, SubjectLarge subject)
        {
            item.Eps = new List<SimpleEp>();
            if (subject?.Eps != null)
            {
                foreach (var ep in subject.Eps)
                {
                    SimpleEp simpleEp = new SimpleEp
                    {
                        Id = ep.Id,
                        Sort = ep.Sort,
                        Status = ep.Status,
                        Type = ep.Type,
                        Name = ep.NameCn == "" ? ep.Name : ep.NameCn,
                        AirDate = DateTime.TryParse(ep.AirDate, out var d) ? d : d
                    };
                    item.Eps.Add(simpleEp);
                }
                item.UpdatedEps = item.Eps.Count(ep => ep.Type == 0 && Regex.IsMatch(ep.Status, "(Air|Today)"));
            }
            else
            {
                item.WatchedEps = -1;
                item.UpdatedEps = -1;
            }

        }

        /// <summary>
        /// 处理用户进度
        /// </summary>
        /// <param name="item">显示条目</param>
        /// <param name="progress">进度</param>
        private void ProcessProgress(WatchStatus item, Progress progress)
        {
            if (progress?.Eps != null)
            {
                // 填充用户观看状态
                foreach (var ep in item.Eps)
                {
                    var temp = progress.Eps.ToList();
                    foreach (var p in temp)
                    {
                        if (p.Id == ep.Id)
                        {
                            ep.Status = p.Status.CnName;
                            temp.Remove(p);
                            break;
                        }
                    }
                }
                item.WatchedEps = item.Eps.Count(ep => ep.Type == 0 && !Regex.IsMatch(ep.Status, "(Air|Today|NA)"));
                if (item.Eps.Count(ep => ep.Type == 0) == item.WatchedEps)
                {
                    item.NextEp = -1;
                }
                if (item.Eps.FirstOrDefault(ep => ep.Type == 0 && Regex.IsMatch(ep.Status, "(Air|Today)")) != null)
                {
                    item.EpColor = "#d26585";
                }
                else
                {
                    item.EpColor = "Gray";
                }
            }
            else
            {
                item.WatchedEps = 0;
                if (item.UpdatedEps != 0)
                {
                    item.EpColor = "#d26585";
                }
            }

            if (item.NextEp != -1 && item.Eps.Count != 0)
            {
                item.NextEp = item.Eps.Where(ep => ep.Type == 0 && Regex.IsMatch(ep.Status, "(Air|Today)")).OrderBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                              item.Eps.Where(ep => ep.Type == 0 && Regex.IsMatch(ep.Status, "(NA)")).OrderBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                              -1;
            }
        }

        #endregion

        /// <summary>
        /// 以新列表为准，将老列表改为与新列表相同
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin">显示的列表</param>
        /// <param name="dest">新的列表</param>
        private void DiffListToObservableCollection<T>(ObservableCollection<T> origin, List<T> dest) where T : class
        {
            if (!origin.SequenceEqualExT(dest))
            {
                int compareCount = 0;
                for (int i = 0; i < dest.Count; i++)
                {
                    bool insert = true;
                    for (int j = i; j < origin.Count; j++)
                    {
                        compareCount++;
                        if (dest[i].EqualsExT(origin[j]))
                        {
                            if (j != i)
                            {
                                origin.Move(j, i);
                            }
                            insert = false;
                            break;
                        }
                        else
                        {
                            bool removed = true;
                            for (int k = j; k < dest.Count; k++)
                            {
                                compareCount++;
                                if (origin[j].EqualsExT(dest[k]))
                                {
                                    removed = false;
                                    break;
                                }
                            }
                            if (removed)
                            {
                                origin.RemoveAt(j--);
                            }
                        }
                    }
                    if (insert)
                    {
                        origin.Insert(i, dest[i]);
                    }
                }
                NotificationHelper.Notify($"{nameof(compareCount)}: {compareCount}", NotificationHelper.NotifyType.Debug);
                // 若通过以上步骤无法排好序，则重置列表
                if (!origin.SequenceEqualExT(dest))
                {
                    NotificationHelper.Notify($"{nameof(compareCount)}: {compareCount}\n比较失败，重置列表！", NotificationHelper.NotifyType.Debug);
                    origin.Clear();
                    foreach (var item in dest)
                    {
                        origin.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// 对条目进行排序
        /// </summary>
        private List<WatchStatus> CollectionSorting(IEnumerable<WatchStatus> watchingStatuses)
        {
            return watchingStatuses.OrderBy(p => p.EpColor)
                                   .ThenBy(p => p.WatchedEps == 0)
                                   .ThenBy(p => p.UpdatedEps - p.WatchedEps)
                                   .ThenBy(p => p.Eps?.LastOrDefault(ep => ep.Type == 0 && !Regex.IsMatch(ep.Status, "(NA)"))?.AirDate)
                                   .ThenBy(p =>
                                   {
                                       if (DateTime.TryParse(p.AirTime, out var airTime))
                                       {
                                           var first = p.Eps?.FirstOrDefault(ep => ep.Type == 0)?.AirDate;
                                           var last = p.Eps?.LastOrDefault(ep => ep.Type == 0 && !Regex.IsMatch(ep.Status, "(NA)"))?.AirDate;
                                           if (first != null && last != null)
                                           {
                                               return airTime.AddTicks(last.Value.Ticks).AddTicks(-first.Value.Ticks);
                                           }
                                       }
                                       return airTime;
                                   })
                                   .ToList();
        }

        #endregion

    }


    public class WatchStatus : ViewModelBase
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
        public int EpsCount { get => Eps.Count(ep => ep.Type == 0); }
        public List<SimpleEp> Eps { get; set; }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set => Set(ref _isUpdating, value);
        }

        private int _watchedEps;
        public int WatchedEps
        {
            get => _watchedEps;
            set => Set(ref _watchedEps, value);
        }

        private int _updatedEps;
        public int UpdatedEps
        {
            get => _updatedEps;
            set => Set(ref _updatedEps, value);
        }

        private double _nextEp;
        public double NextEp
        {
            get => _nextEp;
            set => Set(ref _nextEp, value);
        }

        private string _epColor;
        public string EpColor
        {
            get => _epColor;
            set => Set(ref _epColor, value);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            WatchStatus w = (WatchStatus)obj;
            return SubjectId == w.SubjectId &&
                   LastTouch == w.LastTouch &&
                   Type == w.Type &&
                   AirTime == w.AirTime &&
                   IsUpdating == w.IsUpdating &&
                   WatchedEps == w.WatchedEps &&
                   UpdatedEps == w.UpdatedEps &&
                   NextEp == w.NextEp &&
                   EpColor.EqualsExT(w.EpColor) &&
                   Name.EqualsExT(w.Name) &&
                   NameCn.EqualsExT(w.NameCn) &&
                   Url.EqualsExT(w.Url) &&
                   Image.EqualsExT(w.Image) &&
                   Eps.SequenceEqualExT(w.Eps);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (int)(SubjectId + LastTouch % 1000000000) - UpdatedEps;
        }
    }

    public class SimpleEp
    {
        public int Id { get; set; }
        public EpisodeType Type { get; set; }
        public double Sort { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public DateTime AirDate { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SimpleEp s = (SimpleEp)obj;
            return Id == s.Id &&
                   Type == s.Type &&
                   Sort == s.Sort &&
                   Status.EqualsExT(s.Status) &&
                   Name.EqualsExT(s.Name) &&
                   AirDate == s.AirDate;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
