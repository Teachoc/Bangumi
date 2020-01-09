using Bangumi.Api;
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

        private readonly object lockObj = new object();

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
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        /// <param name="status"></param>
        /// <param name="currentStatus"></param>
        public async void EditCollectionStatus(WatchStatus status, CollectionStatusEnum currentStatus = CollectionStatusEnum.Do)
        {
            var subjectStatus = BangumiApi.GetSubjectStatusAsync(status.SubjectId.ToString());
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog(
                (SubjectTypeEnum)status.Type, subjectStatus.Item2)
            {
                Title = status.NameCn,
            };
            MainPage.RootPage.HasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync())
            {
                status.IsUpdating = true;
                if (await BangumiFacade.UpdateCollectionStatusAsync(status.SubjectId.ToString(),
                                                                    collectionEditContentDialog.CollectionStatus,
                                                                    collectionEditContentDialog.Comment,
                                                                    collectionEditContentDialog.Rate.ToString(),
                                                                    collectionEditContentDialog.Privacy ? "1" : "0"))
                {
                    // 若修改后状态不是在看，则从进度页面删除
                    if (collectionEditContentDialog.CollectionStatus != CollectionStatusEnum.Do)
                    {
                        WatchingCollection.Remove(status);
                    }
                }
                status.IsUpdating = false;
            }
            MainPage.RootPage.HasDialog = false;
        }

        /// <summary>
        /// 刷新收视进度列表。
        /// </summary>
        /// <param name="fromCache">只从缓存加载</param>
        /// <returns></returns>
        public async Task LoadWatchingListAsync(BangumiApi.RequestType requestType)
        {
            try
            {
                if (BangumiApi.IsLogin)
                {
                    IsLoading = true;
                    await PopulateWatchingListAsync(WatchingCollection, requestType);
                }
                else
                {
                    //Message = "请先登录！";
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("获取收视进度列表失败。");
                Debug.WriteLine(e.Message);
                NotificationHelper.Notify("获取收视进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 更新下一章章节状态为已看
        /// </summary>
        /// <param name="item"></param>
        public async void UpdateNextEpStatus(WatchStatus item)
        {
            if (item?.Eps != null && item.Eps.Count != 0 && item.NextEp != -1)
            {
                item.IsUpdating = true;
                if (await BangumiFacade.UpdateProgressAsync(item.Eps.FirstOrDefault(ep => Regex.IsMatch(ep.Status, "(Air|Today|NA)") &&
                                                                                          ep.Sort == item.NextEp).Id.ToString(),
                                                            EpStatusEnum.watched))
                {
                    item.Eps.FirstOrDefault(ep => Regex.IsMatch(ep.Status, "(Air|Today|NA)") && ep.Sort == item.NextEp).Status = "看过";
                    item.WatchedEps++;
                    item.NextEp = item.Eps.Where(ep => Regex.IsMatch(ep.Status, "(Air|Today)")).OrderBy(ep => ep.Type).ThenBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                                  item.Eps.Where(ep => Regex.IsMatch(ep.Status, "(NA)")).OrderBy(ep => ep.Type).ThenBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                                  -1;
                    // 若未看到最新一集，则使用粉色，否则使用灰色
                    if (item.Eps.FirstOrDefault(ep => Regex.IsMatch(ep.Status, "(Air|Today)")) != null)
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
                        if (SettingHelper.SubjectComplete && item.Eps.FirstOrDefault(ep => Regex.IsMatch(ep.Status, "(Air|Today|NA)")) == null)
                        {
                            EditCollectionStatus(item, CollectionStatusEnum.Collect);
                        }
                    }
                    item.LastTouch = DateTime.Now.ConvertDateTimeToJsTick();
                }
                item.IsUpdating = false;
            }
        }

        #endregion

        #region 私有方法

        #region 进度
        /// <summary>
        /// 处理并显示用户收视进度列表。
        /// </summary>
        /// <param name="watchingCollection"></param>
        /// <returns></returns>
        private async Task PopulateWatchingListAsync(ObservableCollection<WatchStatus> watchingCollection,
            BangumiApi.RequestType requestType = BangumiApi.RequestType.All)
        {
            var watchings = BangumiApi.GetWatchingListAsync();
            var cachedWatchings = watchings.Item1;
            var cachedWatchList = new List<WatchStatus>();
            var currentWatchList = new List<WatchStatus>();
            // 加载缓存，后面获取新数据后比较需要使用
            foreach (var watching in cachedWatchings)
            {
                var subject = BangumiApi.GetSubjectEpsAsync(watching.SubjectId.ToString(), BangumiApi.RequestType.CacheOnly);
                var progress = BangumiApi.GetProgressesAsync(watching.SubjectId.ToString(), BangumiApi.RequestType.CacheOnly);

                var item = ProcessWatching(watching);
                ProcessSubject(item, subject.Item1);
                ProcessProgress(item, progress.Item1);
                cachedWatchList.Add(item);
                if (subject.Item1 == null || progress.Item1 == null)
                {
                    watching.LastTouch = 0;
                }
            }
            if (requestType != BangumiApi.RequestType.TaskOnly)
            {
                DiffListToObservableCollection(watchingCollection, CollectionSorting(cachedWatchList));
            }
            if (requestType != BangumiApi.RequestType.CacheOnly)
            {
                // 内容有变更
                await watchings.Item2.ContinueWith(async t =>
                {
                    var subjectTasks = new List<Task<Subject>>();
                    var progressTasks = new List<Task<Progress>>();
                    Subject[] newSubjects = null;
                    Progress[] newProgresses = null;
                    var watchingsNotCached = BangumiApi.IsCacheUpdatedToday ?
                                             t.Result.Where(it => cachedWatchings.All(it2 => !it2.EqualsExT(it))).ToList() :
                                             t.Result;
                    using (var semaphore = new SemaphoreSlim(10))
                    {
                        foreach (var item in watchingsNotCached)
                        {
                            await semaphore.WaitAsync();
                            subjectTasks.Add(BangumiApi.GetSubjectEpsAsync(item.SubjectId.ToString(), BangumiApi.RequestType.TaskOnly).Item2
                                .ContinueWith(t =>
                                {
                                    semaphore.Release();
                                    return t.Result;
                                }));
                            await semaphore.WaitAsync();
                            progressTasks.Add(BangumiApi.GetProgressesAsync(item.SubjectId.ToString(), BangumiApi.RequestType.TaskOnly).Item2
                                .ContinueWith(t =>
                                {
                                    semaphore.Release();
                                    return t.Result;
                                }));
                        }
                        newSubjects = await Task.WhenAll(subjectTasks);
                        newProgresses = await Task.WhenAll(progressTasks);
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
                            item = cachedWatchList.Find(it => it.SubjectId == watching.SubjectId);
                        }
                        currentWatchList.Add(item);
                    }
                    BangumiApi.IsCacheUpdatedToday = true;
                }).Unwrap();
                DiffListToObservableCollection(watchingCollection, CollectionSorting(currentWatchList));
            }
            // 将 Watching 转换为界面显示用的 WatchStatus
            WatchStatus ProcessWatching(Watching w)
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
                        ? BangumiData.GetAirTimeByBangumiId(w.SubjectId.ToString())
                        ?? Converters.GetWeekday(w.Subject.AirWeekday)
                        : Converters.GetWeekday(w.Subject.AirWeekday),
                    Type = w.Subject.Type,
                    IsUpdating = false,
                };
            }
        }

        /// <summary>
        /// 处理条目章节
        /// </summary>
        /// <param name="item">显示条目</param>
        /// <param name="subject">条目章节</param>
        /// <returns></returns>
        private void ProcessSubject(WatchStatus item, Subject subject)
        {
            //item.IsUpdating = true;

            item.Eps = new List<SimpleEp>();
            if (subject?.Eps.Count > 0)
            {
                foreach (var ep in subject.Eps)
                {
                    SimpleEp simpleEp = new SimpleEp
                    {
                        Id = ep.Id,
                        Sort = ep.Sort,
                        Status = ep.Status,
                        Type = ep.Type,
                        Name = ep.NameCn == "" ? ep.Name : ep.NameCn
                    };
                    item.Eps.Add(simpleEp);
                }
                item.UpdatedEps = item.Eps.Count(ep => Regex.IsMatch(ep.Status, "(Air|Today)"));
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
            if (progress != null)
            {
                item.WatchedEps = progress.Eps.Count;
                if (item.Eps.Count == item.WatchedEps)
                {
                    item.NextEp = -1;
                }
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
                if (item.Eps.FirstOrDefault(ep => Regex.IsMatch(ep.Status, "(Air|Today)")) != null)
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
                item.NextEp = item.Eps.Where(ep => Regex.IsMatch(ep.Status, "(Air|Today)")).OrderBy(ep => ep.Type).ThenBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                              item.Eps.Where(ep => Regex.IsMatch(ep.Status, "(NA)")).OrderBy(ep => ep.Type).ThenBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                              -1;
            }
        }

        #endregion

        /// <summary>
        /// 以新列表为准，将老列表改为与新列表相同
        /// 目前效率不高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin">显示的列表</param>
        /// <param name="dest">新的列表</param>
        private void DiffListToObservableCollection<T>(ObservableCollection<T> origin, List<T> dest)
        {
            if (!origin.SequenceEqualExT(dest))
            {
                // 检查原有列表中的项目是否还在，否则删除
                for (int i = 0; i < origin.Count; i++)
                {
                    if (dest.Find(d => d.GetHashCode() == origin[i].GetHashCode()) == null)
                    {
                        origin.Remove(origin[i--]);
                    }
                }
                // 添加新增的
                for (int i = 0; i < dest.Count; i++)
                {
                    if (origin.Where(o => o.GetHashCode() == dest[i].GetHashCode()).FirstOrDefault() == null)
                    {
                        origin.Insert(i, dest[i]);
                    }
                }
                // 调整顺序
                for (int i = 0; i < origin.Count; i++)
                {
                    int index = origin.IndexOf(dest[i]);
                    if (index != i && index >= 0)
                    {
                        origin.Move(index, i);
                    }
                }
                // 若通过以上步骤任无法排好序，则重置列表
                if (!origin.SequenceEqualExT(dest))
                {
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
        private List<WatchStatus> CollectionSorting(List<WatchStatus> watchingStatuses)
        {
            return watchingStatuses.OrderBy(p => p.EpColor)
                                   .ThenBy(p => p.WatchedEps == 0)
                                   .ThenBy(p => p.UpdatedEps - p.WatchedEps)
                                   .ThenBy(p => p.LastTouch)
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
        public int Type { get; set; }
        public string AirTime { get; set; }
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
        public int Type { get; set; }
        public double Sort { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }

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
                   Name.EqualsExT(s.Name);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
