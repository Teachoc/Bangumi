using Bangumi.Api;
using Bangumi.Api.Common;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Controls;
using Bangumi.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bangumi.ViewModels
{
    public class EpisodeViewModel : ViewModelBase
    {
        #region 属性
        public ObservableCollection<GroupedEpisode> GroupedEps { get; private set; } = new ObservableCollection<GroupedEpisode>();

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

        private bool _isDetailLoading;
        public bool IsDetailLoading
        {
            get => _isDetailLoading;
            set => Set(ref _isDetailLoading, value);
        }

        private bool _isProgressLoading;
        public bool IsProgressLoading
        {
            get => _isProgressLoading;
            set => Set(ref _isProgressLoading, value);
        }

        private bool _isStatusLoading;
        public bool IsStatusLoading
        {
            get => _isStatusLoading;
            set => Set(ref _isStatusLoading, value);
        }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set => Set(ref _isUpdating, value);
        }

        private string _subjectId;
        public string SubjectId
        {
            get => _subjectId;
            set => Set(ref _subjectId, value);
        }

        private SubjectType SubjectType { get; set; }

        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set => Set(ref _imageSource, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _airDate;
        public string AirDate
        {
            get => _airDate;
            set => Set(ref _airDate, value);
        }

        private string _airTime;
        public string AirTime
        {
            get => _airTime;
            set => Set(ref _airTime, value);
        }

        private string _summary;
        public string Summary
        {
            get => _summary;
            set => Set(ref _summary, value);
        }

        private double _score;
        public double Score
        {
            get => _score;
            set => Set(ref _score, value);
        }

        public ObservableCollection<SimpleRate> OthersRates { get; private set; } = new ObservableCollection<SimpleRate>();

        private string _othersCollection;
        public string OthersCollection
        {
            get => _othersCollection;
            set => Set(ref _othersCollection, value);
        }

        // 详情
        public DetailViewModel Detail { get; set; }

        // 收藏状态，评分，吐槽
        private CollectionStatusType? _collectionStatus;
        private void SetCollectionStatus(CollectionStatusType? value)
        {
            Set(ref _collectionStatus, value);
            OnPropertyChanged(nameof(CollectionStatusText));
            OnPropertyChanged(nameof(CollectionStatusIcon));
        }

        public string CollectionStatusText => _collectionStatus?.GetDesc(SubjectType) ?? "收藏";

        public string CollectionStatusIcon => _collectionStatus switch
        {
            CollectionStatusType.Dropped => "\uE007",
            null => "\uE006",
            _ => "\uE00B",
        };
        #endregion


        public void InitViewModel()
        {
            IsLoading = false;
            ImageSource = "";
            Name = "";
            AirDate = "";
            AirTime = "";
            Summary = "";
            OthersRates.Clear();
            OthersCollection = "";
            Score = 0;
            // 收藏状态，评分，吐槽
            SetCollectionStatus(null);
            GroupedEps.Clear();
        }


        /// <summary>
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        public async Task EditCollectionStatus()
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (!BangumiApi.BgmOAuth.IsLogin)
            {
                return;
            }
            var subjectStatus = BangumiApi.BgmApi.Status(SubjectId);
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog(
                this.SubjectType, subjectStatus)
            {
                Title = this.Name,
            };
            MainPage.RootPage.HasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync() &&
                collectionEditContentDialog.CollectionStatus != null)
            {
                IsUpdating = true;
                IsStatusLoading = true;
                try
                {
                    var collectionStatusE = await BangumiApi.BgmApi.UpdateStatus(SubjectId,
                        collectionEditContentDialog.CollectionStatus.Value,
                        collectionEditContentDialog.Comment,
                        collectionEditContentDialog.Rate.ToString(),
                        collectionEditContentDialog.Privacy ? "1" : "0");
                    SetCollectionStatus(collectionStatusE.Status.Id);
                    // 若状态修改为看过，且设置启用，则批量修改正片章节状态为看过
                    if (collectionStatusE.Status.Id == CollectionStatusType.Collect && SettingHelper.EpsBatch)
                    {
                        var selectedEps = GroupedEps.SelectMany(g => g.Where(ep => ep.Type == EpisodeType.本篇 && ep.EpStatus == EpStatusType.remove));
                        int epId = selectedEps.LastOrDefault()?.Id ?? 0;
                        string epsId = string.Join(',', selectedEps.Select(it => it.Id));
                        if (!string.IsNullOrEmpty(epsId))
                        {
                            try
                            {
                                if (await BangumiApi.BgmApi.UpdateProgressBatch(epId, epsId))
                                {
                                    foreach (var episode in selectedEps)
                                    {
                                        episode.EpStatus = EpStatusType.watched;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                NotificationHelper.Notify("批量标记章节状态失败！\n错误信息：" + e.Message,
                                                          NotifyType.Error);
                            }
                        }
                        else
                        {
                            NotificationHelper.Notify("无章节需要更新");
                        }
                    }
                }
                catch (Exception e)
                {
                    NotificationHelper.Notify("更新条目状态失败！\n" + e.Message,
                                              NotifyType.Error);
                }
                IsStatusLoading = false;
                IsUpdating = false;
            }
            MainPage.RootPage.HasDialog = false;
        }

        /// <summary>
        /// 更新章节状态
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status">状态</param>
        public async Task UpdateEpStatus(EpisodeWithEpStatus ep, EpStatusType status)
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (ep != null)
            {
                try
                {
                    IsUpdating = true;
                    if (await BangumiApi.BgmApi.UpdateProgress(ep.Id.ToString(), status))
                    {
                        ep.EpStatus = status;
                        NotificationHelper.Notify($"标记 ep.{ep.Sort} {Converters.StringOneOrTwo(ep.NameCn, ep.Name)} {status.GetCnName()}成功");
                    }
                    else
                    {
                        NotificationHelper.Notify($"标记 ep.{ep.Sort} {Converters.StringOneOrTwo(ep.NameCn, ep.Name)} {status.GetCnName()}失败，请重试！",
                                                  NotifyType.Warn);
                    }
                }
                catch (Exception e)
                {
                    NotificationHelper.Notify($"标记 ep.{ep.Sort} {Converters.StringOneOrTwo(ep.NameCn, ep.Name)} {status.GetCnName()}失败！\n错误信息：{e.Message}",
                                              NotifyType.Error);
                }
                finally
                {
                    IsUpdating = false;
                }
            }
        }

        /// <summary>
        /// 批量更新章节状态为看过
        /// </summary>
        /// <param name="ep"></param>
        public async Task UpdateEpStatusBatch(EpisodeWithEpStatus ep)
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (ep == null)
            {
                return;
            }
            IsUpdating = true;
            var selectedEps = GetSelectedEps();
            string epsId = string.Join(',', selectedEps.Select(it => it.Id.ToString()));
            if (!string.IsNullOrEmpty(epsId))
            {
                try
                {
                    if (await BangumiApi.BgmApi.UpdateProgressBatch(ep.Id, epsId))
                    {
                        foreach (var episode in selectedEps)
                        {
                            episode.EpStatus = EpStatusType.watched;
                        }
                    }
                }
                catch (Exception e)
                {
                    NotificationHelper.Notify("批量标记章节状态失败！\n错误信息：" + e.Message,
                                              NotifyType.Error);
                }
            }
            else
            {
                NotificationHelper.Notify("无章节需要更新");
            }
            IsUpdating = false;

            // 获取点击看到章节及之前所有未标记的章节
            IEnumerable<EpisodeWithEpStatus> GetSelectedEps()
            {
                foreach (var episode in GroupedEps.SelectMany(g => g.Where(e => e.Type == ep.Type)))
                {
                    if (episode.EpStatus == EpStatusType.remove)
                    {
                        yield return episode;
                    }
                    if (episode.Id == ep.Id)
                    {
                        yield break;
                    }
                }
            }
        }

        /// <summary>
        /// 加载详情和章节，
        /// 用户进度，收藏状态。
        /// </summary>
        public async Task LoadDetails()
        {
            if (NetworkHelper.IsOffline)
            {
                ProcessSubject(BangumiApi.BgmCache.Subject(SubjectId));
                ProcessProgress(BangumiApi.BgmCache.Progress(SubjectId));
                SetCollectionStatus(BangumiApi.BgmCache.Status(SubjectId)?.Status?.Id);
                return;
            }
            if (IsLoading)
            {
                return;
            }
            try
            {
                IsLoading = true;
                IsDetailLoading = true;
                IsProgressLoading = true;
                IsStatusLoading = true;

                var subject = BangumiApi.BgmApi.Subject(SubjectId);
                ProcessSubject(BangumiApi.BgmCache.Subject(SubjectId));
                // 检查用户登录状态
                if (BangumiApi.BgmOAuth.IsLogin)
                {
                    var progress = BangumiApi.BgmApi.Progress(SubjectId);
                    var status = BangumiApi.BgmApi.Status(SubjectId);
                    ProcessProgress(BangumiApi.BgmCache.Progress(SubjectId));
                    SetCollectionStatus(BangumiApi.BgmCache.Status(SubjectId)?.Status?.Id);
                    ProcessSubject(await subject);
                    IsDetailLoading = false;
                    ProcessProgress(await progress);
                    IsProgressLoading = false;
                    SetCollectionStatus((await status)?.Status?.Id);
                    IsStatusLoading = false;
                }
                else
                {
                    ProcessSubject(await subject);
                    IsDetailLoading = false;
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("加载详情失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotifyType.Error);
            }
            finally
            {
                IsLoading = false;
                IsDetailLoading = false;
                IsProgressLoading = false;
                IsStatusLoading = false;
            }
        }

        /// <summary>
        /// 处理条目信息
        /// </summary>
        /// <param name="subject"></param>
        private void ProcessSubject(SubjectLarge subject)
        {
            if (subject == null || subject.Id.ToString() != SubjectId)
            {
                return;
            }
            // 条目标题
            Name = Converters.StringOneOrTwo(subject.NameCn, subject.Name);
            // 条目图片
            ImageSource = subject.Images?.Common;
            // 放送日期
            AirDate = subject.AirDate;
            AirTime = subject.AirWeekdayCn;
            // 条目简介
            if (!string.IsNullOrEmpty(subject.Summary))
            {
                Summary = subject.Summary.Length > 80 ? (subject.Summary.Substring(0, 80) + "...") : subject.Summary;
            }
            else
            {
                Summary = "暂无简介";
            }
            if (subject.Rating != null)
            {
                Score = subject.Rating.Score;
                var simpleRates = new List<SimpleRate>
                {
                    new SimpleRate {Count = subject.Rating.Count._10, Score = 10},
                    new SimpleRate {Count = subject.Rating.Count._9, Score = 9},
                    new SimpleRate {Count = subject.Rating.Count._8, Score = 8},
                    new SimpleRate {Count = subject.Rating.Count._7, Score = 7},
                    new SimpleRate {Count = subject.Rating.Count._6, Score = 6},
                    new SimpleRate {Count = subject.Rating.Count._5, Score = 5},
                    new SimpleRate {Count = subject.Rating.Count._4, Score = 4},
                    new SimpleRate {Count = subject.Rating.Count._3, Score = 3},
                    new SimpleRate {Count = subject.Rating.Count._2, Score = 2},
                    new SimpleRate {Count = subject.Rating.Count._1, Score = 1}
                };
                double sumCount = simpleRates.Sum(s => s.Count);
                OthersRates.Clear();
                foreach (var item in simpleRates)
                {
                    item.Ratio = (double)item.Count * 100 / sumCount;
                    OthersRates.Add(item);
                }
            }
            if (subject.Collection != null)
            {
                OthersCollection = subject.Collection.Wish + "想看/" +
                                   subject.Collection.Collect + "看过/" +
                                   subject.Collection.Doing + "在看/" +
                                   subject.Collection.OnHold + "搁置/" +
                                   subject.Collection.Dropped + "抛弃";
            }

            SubjectType = subject.Type;
            // 详情
            string info = "作品分类：" + subject.Type.GetDesc();
            info += subject.AirDate == "0000-00-00" ? "" : "\n放送开始：" + subject.AirDate;
            info += subject.AirWeekday == 0 ? "" : "\n放送星期：" + subject.AirWeekdayCn;
            info += subject.Eps == null ? "" : "\n话数：" + subject.Eps.Count;
            Detail = new DetailViewModel
            {
                Name = subject.Name,
                Summary = subject.Summary,
                Info = info,
                Characters = subject.Characters,
                Staffs = subject.Staff,
                Blogs = subject.Blogs,
                Topics = subject.Topics
            };

            // 章节
            if (subject.Eps != null)
            {
                var eps = new List<EpisodeWithEpStatus>();
                foreach (var ep in subject.Eps)
                {
                    eps.Add(EpisodeWithEpStatus.FromEpisode(ep));
                }
                var groupEps = eps.GroupBy(ep => ep.Type)
                    .OrderBy(g => g.Key)
                    .Select(g => new GroupedEpisode(g) { Key = g.Key.GetDesc() });
                if (!groupEps.SequenceEqualExT(GroupedEps))
                {
                    GroupedEps.Clear();
                    foreach (var item in groupEps)
                    {
                        GroupedEps.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// 处理用户章节状态
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="progress"></param>
        private void ProcessProgress(Progress progress)
        {
            if (progress?.Eps == null || progress.SubjectId.ToString() != SubjectId)
            {
                return;
            }
            foreach (var group in GroupedEps) //用户观看状态
            {
                foreach (EpisodeWithEpStatus ep in group)
                {
                    var prog = progress.Eps?.Where(p => p.Id == ep.Id).FirstOrDefault();
                    ep.EpStatus = prog?.Status?.Id ?? EpStatusType.remove;
                }
            }
        }
    }

    public class GroupedEpisode : List<EpisodeWithEpStatus>
    {
        public GroupedEpisode(IEnumerable<EpisodeWithEpStatus> items) : base(items)
        {
        }
        public string Key { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var g = (GroupedEpisode)obj;
            return Key == g.Key &&
                   this.SequenceEqualExT(g);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class SimpleRate
    {
        public int Count { get; set; }
        public int Score { get; set; }
        public double Ratio { get; set; }
    }
}
