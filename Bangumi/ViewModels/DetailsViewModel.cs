using Bangumi.Api;
using Bangumi.Api.Common;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bangumi.ViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        #region 属性
        public ObservableCollection<Episode> Eps { get; private set; } = new ObservableCollection<Episode>();

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

        private bool _isStatusLoaded;
        public bool IsStatusLoaded
        {
            get => _isStatusLoaded;
            set => Set(ref _isStatusLoaded, value);
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

        private string _nameCn;
        public string NameCn
        {
            get => _nameCn;
            set => Set(ref _nameCn, value);
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



        // 更多资料用
        public ObservableCollection<Character> Characters { get; private set; } = new ObservableCollection<Character>();
        public ObservableCollection<Person> Staffs { get; private set; } = new ObservableCollection<Person>();
        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        private string _moreInfo;
        public string MoreInfo
        {
            get => _moreInfo;
            set => Set(ref _moreInfo, value);
        }
        private string _moreSummary;
        public string MoreSummary
        {
            get => _moreSummary;
            set => Set(ref _moreSummary, value);
        }

        // 评论和讨论版
        public ObservableCollection<Blog> Blogs { get; private set; } = new ObservableCollection<Blog>();
        public ObservableCollection<Topic> Topics { get; private set; } = new ObservableCollection<Topic>();

        // 收藏状态，评分，吐槽
        private CollectionStatusType? _collectionStatus;
        private void SetCollectionStatus(CollectionStatusType? value)
        {
            Set(ref _collectionStatus, value);
            OnPropertyChanged(nameof(CollectionStatusText));
            OnPropertyChanged(nameof(CollectionStatusIcon));
        }

        public string CollectionStatusText
        {
            get => _collectionStatus?.GetDesc(SubjectType) ?? "收藏";
        }

        public string CollectionStatusIcon
        {
            get
            {
                return _collectionStatus switch
                {
                    CollectionStatusType.Dropped => "\uE007",
                    null => "\uE006",
                    _ => "\uE00B",
                };
            }
        }
        #endregion

        public DetailsViewModel()
        {
            InitViewModel();
        }

        public void InitViewModel()
        {
            IsLoading = false;
            ImageSource = "";
            NameCn = "";
            AirDate = "";
            AirTime = "";
            Summary = "";
            OthersRates.Clear();
            OthersCollection = "";
            Score = 0;
            // 更多资料用
            Characters.Clear();
            Staffs.Clear();
            Name = "";
            MoreInfo = "";
            MoreSummary = "";
            // 收藏状态，评分，吐槽
            SetCollectionStatus(null);
            Blogs.Clear();
            Topics.Clear();
            Eps.Clear();
        }


        /// <summary>
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        public async Task EditCollectionStatus()
        {
            if (!BangumiApi.BgmOAuth.IsLogin)
            {
                return;
            }
            var subjectStatus = BangumiApi.BgmApi.Status(SubjectId);
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog(
                this.SubjectType, subjectStatus)
            {
                Title = this.NameCn,
            };
            MainPage.RootPage.HasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync() &&
                collectionEditContentDialog.CollectionStatus != null)
            {
                IsUpdating = true;
                IsStatusLoaded = false;
                if (await BangumiFacade.UpdateCollectionStatusAsync(SubjectId,
                    collectionEditContentDialog.CollectionStatus.Value, collectionEditContentDialog.Comment,
                    collectionEditContentDialog.Rate.ToString(), collectionEditContentDialog.Privacy ? "1" : "0"))
                {
                    SetCollectionStatus(collectionEditContentDialog.CollectionStatus);
                    // 若状态修改为看过，且设置启用，则批量修改正片章节状态为看过
                    if (_collectionStatus == CollectionStatusType.Collect && SettingHelper.EpsBatch)
                    {
                        var selectedEps = Eps.Where(ep => ep.Type == 0 && Regex.IsMatch(ep.Status, "(Air|Today|NA)"));
                        int epId = selectedEps.LastOrDefault()?.Id ?? 0;
                        string epsId = string.Join(',', selectedEps.Select(it => it.Id));
                        if (!string.IsNullOrEmpty(epsId) && await BangumiFacade.UpdateProgressBatchAsync(epId, epsId))
                        {
                            foreach (var episode in selectedEps)
                            {
                                episode.Status = "看过";
                            }
                        }
                        else
                        {
                            NotificationHelper.Notify("无章节需要更新");
                        }
                    }
                }
                IsStatusLoaded = true;
                IsUpdating = false;
            }
            MainPage.RootPage.HasDialog = false;
        }

        /// <summary>
        /// 更新章节状态
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status">状态</param>
        public async Task UpdateEpStatus(Episode ep, EpStatusType status)
        {
            if (ep != null)
            {
                IsUpdating = true;
                if (await BangumiFacade.UpdateProgressAsync(ep.Id.ToString(), status))
                {
                    if (!string.IsNullOrEmpty(status.GetCnName()))
                    {
                        ep.Status = status.GetCnName();
                    }
                    else
                    {
                        ep.Status = DateTime.Parse(ep.AirDate) < DateTime.Now ? "Air" : "NA";
                    }
                }
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 批量更新章节状态为看过
        /// </summary>
        /// <param name="ep"></param>
        public async Task UpdateEpStatusBatch(Episode ep)
        {
            if (ep == null)
            {
                return;
            }
            IsUpdating = true;
            var selectedEps = GetSelectedEps();
            string epsId = string.Join(',', selectedEps.Select(it => it.Id.ToString()));
            if (!string.IsNullOrEmpty(epsId) && await BangumiFacade.UpdateProgressBatchAsync(ep.Id, epsId))
            {
                foreach (var episode in selectedEps)
                {
                    episode.Status = "看过";
                }
            }
            else
            {
                NotificationHelper.Notify("无章节需要更新");
            }
            IsUpdating = false;

            // 获取点击看到章节及之前所有未标记的章节
            IEnumerable<Episode> GetSelectedEps()
            {
                foreach (var episode in Eps)
                {
                    if (Regex.IsMatch(episode.Status, "(Air|Today|NA)"))
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
            if (IsLoading)
            {
                return;
            }
            try
            {
                IsLoading = true;
                IsDetailLoading = true;
                IsProgressLoading = true;
                IsStatusLoaded = false;

                var subject = BangumiApi.BgmApi.Subject(SubjectId);
                ProcessSubject(BangumiApi.BgmCache.Subject(SubjectId));
                // 检查用户登录状态
                if (BangumiApi.BgmOAuth.IsLogin)
                {
                    var progress = BangumiApi.BgmApi.Progress(SubjectId);
                    var status = BangumiApi.BgmApi.Status(SubjectId);
                    ProcessProgress(BangumiApi.BgmCache.Subject(SubjectId), BangumiApi.BgmCache.Progress(SubjectId));
                    SetCollectionStatus(BangumiApi.BgmCache.Status(SubjectId)?.Status?.Id);
                    await subject.ContinueWith(async t =>
                    {
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                        {
                            ProcessSubject(t.Result);
                            IsDetailLoading = false;
                        });
                        await progress.ContinueWith(t2 =>
                            DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                            {
                                ProcessProgress(t.Result, t2.Result);
                                IsProgressLoading = false;
                            })).Unwrap();
                        await status.ContinueWith(t3 =>
                            DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                            {
                                SetCollectionStatus(t3.Result?.Status?.Id);
                                IsStatusLoaded = true;
                            })).Unwrap();
                    }).Unwrap();
                }
                else
                {
                    await subject.ContinueWith(t =>
                        DispatcherHelper.ExecuteOnUIThreadAsync(() => ProcessSubject(t.Result))).Unwrap();
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("加载详情失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
            }
            finally
            {
                IsLoading = false;
                IsDetailLoading = false;
                IsProgressLoading = false;
                IsStatusLoaded = true;
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
            NameCn = string.IsNullOrEmpty(subject.NameCn) ? subject.Name : subject.NameCn;
            // 条目图片
            ImageSource = subject.Images?.Common;
            // 放送日期
            AirDate = subject.AirDate;
            AirTime = Converters.GetWeekday(subject.AirWeekday);
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

            // 条目类别
            SubjectType = (SubjectType)subject.Type;
            // 更多资料
            Name = subject.Name;
            MoreSummary = subject.Summary;
            MoreInfo = "作品分类：" + SubjectType.GetDesc();
            MoreInfo += subject.AirDate == "0000-00-00" ? "" : "\n放送开始：" + subject.AirDate;
            MoreInfo += subject.AirWeekday == 0 ? "" : "\n放送星期：" + Converters.GetWeekday(subject.AirWeekday);
            MoreInfo += subject.Eps == null ? "" : "\n话数：" + subject.Eps.Count;
            // 角色
            Characters.Clear();
            if (subject.Characters != null)
            {
                foreach (var crt in subject.Characters)
                {
                    Characters.Add(crt);
                }
            }
            // 演职人员
            Staffs.Clear();
            if (subject.Staff != null)
            {
                foreach (var staff in subject.Staff)
                {
                    Staffs.Add(staff);
                }
            }
            // 评论
            Blogs.Clear();
            if (subject.Blogs != null)
            {
                foreach (var blog in subject.Blogs)
                {
                    Blogs.Add(blog);
                }
            }
            // 讨论版
            Topics.Clear();
            if (subject.Topics != null)
            {
                foreach (var topic in subject.Topics)
                {
                    Topics.Add(topic);
                }
            }
            // 章节
            if (!subject.Eps.SequenceEqualExT(Eps))
            {
                Eps.Clear();
                if (subject.Eps != null)
                {
                    foreach (var ep in subject.Eps)
                    {
                        Episode newEp = new Episode
                        {
                            Id = ep.Id,
                            Url = ep.Url,
                            Type = ep.Type,
                            Sort = ep.Sort,
                            Name = ep.Name,
                            NameCn = ep.NameCn,
                            Duration = ep.Duration,
                            AirDate = ep.AirDate,
                            Comment = ep.Comment,
                            Desc = ep.Desc,
                            Status = ep.Status
                        };
                        Eps.Add(newEp);
                    }
                }
            }
        }

        /// <summary>
        /// 处理用户章节状态
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="progress"></param>
        private void ProcessProgress(SubjectLarge subject, Progress progress)
        {
            if (progress?.Eps == null || progress.SubjectId.ToString() != SubjectId)
            {
                return;
            }
            foreach (var ep in Eps) //用户观看状态
            {
                var prog = progress.Eps?.Where(p => p.Id == ep.Id).FirstOrDefault();
                if (prog != null)
                {
                    ep.Status = prog.Status.CnName;
                }
                else
                {
                    ep.Status = subject.Eps.FirstOrDefault(e => e.Id == ep.Id)?.Status;
                }
            }
        }
    }

    public class SimpleRate : IComparable<SimpleRate>
    {

        public int Count { get; set; }
        public int Score { get; set; }
        public double Ratio { get; set; }

        public int CompareTo(SimpleRate other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            return Count.CompareTo(other.Count);
        }
    }
}
