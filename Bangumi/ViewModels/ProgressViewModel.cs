using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Api.Utils;
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

        public ObservableCollection<WatchingStatus> WatchingCollection { get; private set; } = new ObservableCollection<WatchingStatus>();

        private object lockObj = new object();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                Set(ref _isLoading, value);
                HomePage.homePage.isLoading = value;
                MainPage.RootPage.RefreshAppBarButton.IsEnabled = !value;
            }
        }


        #region 公开方法

        /// <summary>
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        /// <param name="status"></param>
        /// <param name="currentStatus"></param>
        public async void EditCollectionStatus(WatchingStatus status, CollectionStatusEnum currentStatus = CollectionStatusEnum.Do)
        {
            BangumiApi.BangumiCache.SubjectStatus.TryGetValue(status.SubjectId.ToString(), out SubjectStatus2 subjectStatus);
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog()
            {
                Rate = subjectStatus?.Rating ?? 0,
                Comment = subjectStatus?.Comment,
                Privacy = subjectStatus?.Private?.Equals("1") ?? false,
                CollectionStatus = currentStatus,
                SubjectType = (SubjectTypeEnum)status.Type,
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
                                                                    collectionEditContentDialog.Privacy == true ? "1" : "0"))
                {
                    // 若修改后状态不是在看，则从进度页面删除
                    if (collectionEditContentDialog.CollectionStatus != CollectionStatusEnum.Do)
                        WatchingCollection.Remove(status);
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
        public async Task LoadWatchingListAsync(bool fromCache)
        {
            try
            {
                if (BangumiApi.IsLogin)
                {
                    IsLoading = true;
                    await PopulateWatchingListAsync(WatchingCollection, fromCache);
                }
                else
                {
                    //Message = "请先登录！";
                }
            }
            catch (Exception e)
            {
                if (e.Message.Equals("401"))
                {
                    // 授权过期，返回登录界面
                    MainPage.RootFrame.Navigate(typeof(LoginPage), "ms-appx:///Assets/resource/err_401.png");
                }
                else
                {
                    Debug.WriteLine("获取收视进度列表失败。");
                    Debug.WriteLine(e.Message);
                    NotificationHelper.Notify("获取收视进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                              NotificationHelper.NotifyType.Error);
                }
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
        public async void UpdateNextEpStatus(WatchingStatus item)
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
                    item.NextEp = item.Eps.Where(ep => Regex.IsMatch(ep.Status, "(Air|Today)")).OrderBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                                  item.Eps.Where(ep => Regex.IsMatch(ep.Status, "(NA)")).OrderBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
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
        /// 显示用户收视进度列表。
        /// </summary>
        /// <param name="watchingCollection"></param>
        /// <param name="fromCache">只从缓存加载</param>
        /// <returns></returns>
        private async Task PopulateWatchingListAsync(ObservableCollection<WatchingStatus> watchingCollection, bool fromCache)
        {
            try
            {
                List<Watching> watchingsCache = BangumiApi.BangumiCache.Watchings.ToList();
                // 加载缓存
                var cachedList = await ProcessWatchings(watchingsCache);
                DiffListToObservableCollection(watchingCollection, cachedList);
                if (!fromCache)
                {
                    var watchings = await BangumiApi.GetWatchingListAsync();
                    // 当天首次更新或内容有变更
                    if (!BangumiApi.IsCacheUpdatedToday || !watchingsCache.SequenceEqualExT(watchings))
                    {
                        var newList = await ProcessWatchings(watchings, watchingCollection.ToList(), false);
                        DiffListToObservableCollection(watchingCollection, newList);
                        // 当天成功更新
                        if (!BangumiApi.IsCacheUpdatedToday)
                        {
                            BangumiApi.IsCacheUpdatedToday = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 处理用户收视进度列表
        /// </summary>
        /// <param name="watchings">收视列表</param>
        /// <param name="cachedWatchings">缓存的收视列表</param>
        /// <param name="fromCache">是否从缓存加载</param>
        /// <returns></returns>
        private async Task<List<WatchingStatus>> ProcessWatchings(List<Watching> watchings, List<WatchingStatus> cachedWatchings = null, bool fromCache = true)
        {
            List<WatchingStatus> watchList = new List<WatchingStatus>();

            var tasks = new List<Task>();
            // semaphore, allow to run 10 tasks in parallel
            using (var semaphore = new SemaphoreSlim(10))
            {
                foreach (var watching in watchings)
                {
                    await semaphore.WaitAsync();

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var item = new WatchingStatus
                            {
                                Name = watching.Subject.Name,
                                NameCn = watching.Subject.NameCn,
                                Image = watching.Subject.Images.Common,
                                SubjectId = watching.SubjectId,
                                Url = watching.Subject.Url,
                                EpColor = "Gray",
                                LastTouch = watching.LastTouch, // 该条目上次修改时间
                                WatchedEps = watching.EpStatus,
                                UpdatedEps = watching.Subject.EpsCount,
                                AirTime = SettingHelper.UseBangumiDataAirTime
                                          ? BangumiData.GetAirTimeByBangumiId(watching.SubjectId.ToString()) ?? Converters.GetWeekday(watching.Subject.AirWeekday)
                                          : Converters.GetWeekday(watching.Subject.AirWeekday),
                                Type = watching.Subject.Type,
                                IsUpdating = false,
                            };


                            // 加载缓存
                            if (fromCache && BangumiApi.BangumiCache.Subjects.TryGetValue(item.SubjectId.ToString(), out Subject subjectCache))
                            {
                                await ProcessSubject(item, subjectCache, fromCache);
                            }
                            else
                            {
                                // 将条目修改时间进行比较，仅更新有修改的条目，以及每天首次更新
                                if (item.LastTouch != cachedWatchings?.Find(c => c.SubjectId == item.SubjectId)?.LastTouch ||
                                    !BangumiApi.IsCacheUpdatedToday)
                                {
                                    var subject = await BangumiApi.GetSubjectEpsAsync(item.SubjectId.ToString());
                                    // 更新数据
                                    await ProcessSubject(item, subject, fromCache);
                                }
                                else
                                {
                                    item = cachedWatchings.Find(c => c.SubjectId == item.SubjectId);
                                }
                            }
                            lock (lockObj)
                            {
                                // 集合的修改操作非线程安全，需加锁
                                watchList.Add(item);
                            }
                        }
                        catch (Exception e)
                        {
                            if (Debugger.IsAttached)
                            {
                                Debugger.Break();
                            }
                            throw e;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));

                }
                // await for the rest of tasks to complete
                await Task.WhenAll(tasks);
                return CollectionSorting(watchList);
            }
        }

        /// <summary>
        /// 处理条目章节
        /// </summary>
        /// <param name="item">显示条目</param>
        /// <param name="subject">条目章节</param>
        /// <param name="fromCache">是否从缓存加载</param>
        /// <returns></returns>
        private async Task ProcessSubject(WatchingStatus item, Subject subject, bool fromCache)
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
                item.UpdatedEps = item.Eps.Where(ep => Regex.IsMatch(ep.Status, "(Air|Today)")).Count();

                if (fromCache && BangumiApi.BangumiCache.Progresses.TryGetValue(item.SubjectId.ToString(), out Progress progressCache))
                {
                    // 加载缓存
                    ProcessProgress(item, progressCache);
                }
                else
                {
                    var progress = await BangumiApi.GetProgressesAsync(item.SubjectId.ToString());
                    // 更新数据
                    ProcessProgress(item, progress);
                }

            }
            else
            {
                item.WatchedEps = -1;
                item.UpdatedEps = -1;
            }

            if (item.NextEp != -1 && item.Eps.Count != 0)
            {
                item.NextEp = item.Eps.Where(ep => Regex.IsMatch(ep.Status, "(Air|Today)")).OrderBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                              item.Eps.Where(ep => Regex.IsMatch(ep.Status, "(NA)")).OrderBy(ep => ep.Sort).FirstOrDefault()?.Sort ??
                              -1;
            }
        }

        /// <summary>
        /// 处理用户进度
        /// </summary>
        /// <param name="item">显示条目</param>
        /// <param name="progress">进度</param>
        private void ProcessProgress(WatchingStatus item, Progress progress)
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
        private List<WatchingStatus> CollectionSorting(List<WatchingStatus> watchingStatuses)
        {
            return watchingStatuses.OrderBy(p => p.EpColor)
                                   .ThenBy(p => p.WatchedEps == 0)
                                   .ThenBy(p => p.UpdatedEps - p.WatchedEps)
                                   .ThenBy(p => p.LastTouch)
                                   .ToList();
        }

        #endregion

    }



    public class WatchingStatus : ViewModelBase
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

        private int _watched_eps;
        public int WatchedEps
        {
            get { return _watched_eps; }
            set => Set(ref _watched_eps, value);
        }

        private int _updated_eps;
        public int UpdatedEps
        {
            get { return _updated_eps; }
            set => Set(ref _updated_eps, value);
        }

        private double _next_ep;
        public double NextEp
        {
            get => _next_ep;
            set => Set(ref _next_ep, value);
        }

        private string _ep_color;
        public string EpColor
        {
            get { return _ep_color; }
            set => Set(ref _ep_color, value);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            WatchingStatus w = (WatchingStatus)obj;
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
            return (int)(LastTouch % 1000000000) - UpdatedEps;
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
