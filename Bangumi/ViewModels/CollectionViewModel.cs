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
    public class CollectionViewModel : ViewModelBase
    {
        public CollectionViewModel()
        {
            IsLoading = false;
        }

        public ObservableCollection<Collection> SubjectCollection { get; set; } = new ObservableCollection<Collection>();

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

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                Set(ref _selectedIndex, value);
                if (!BangumiApi.BgmOAuth.IsLogin)
                {
                    return;
                }
                PopulateSubjectCollectionFromCache();
                PopulateSubjectCollectionAsync();
            }
        }

        /// <summary>
        /// 根据下拉框返回所选择的收藏类型
        /// </summary>
        /// <returns></returns>
        private SubjectType SubjectType => SelectedIndex switch
        {
            0 => SubjectType.Anime,
            1 => SubjectType.Book,
            2 => SubjectType.Music,
            3 => SubjectType.Game,
            4 => SubjectType.Real,
            _ => throw new ArgumentOutOfRangeException("No such subject type.")
        };

        /// <summary>
        /// 显示用户选定类别收藏信息，API限制每类最多25条。
        /// </summary>
        public async Task PopulateSubjectCollectionAsync()
        {
            if (NetworkHelper.IsOffline)
            {
                return;
            }
            try
            {
                IsLoading = true;
                CollectionE current = await BangumiApi.BgmApi.Collections(SubjectType);
                if (!current.Collects.SequenceEqualExT(SubjectCollection))
                {
                    //清空原数据
                    SubjectCollection.Clear();
                    foreach (var status in current.Collects)
                    {
                        SubjectCollection.Add(status);
                    }
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("获取用户收藏失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotifyType.Error);
                Debug.WriteLine(e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void PopulateSubjectCollectionFromCache()
        {
            CollectionE cache = BangumiApi.BgmCache.Collections(SubjectType);
            if (cache != null && !cache.Collects.SequenceEqualExT(SubjectCollection.ToList()))
            {
                //清空原数据
                SubjectCollection.Clear();
                foreach (var status in cache.Collects)
                {
                    SubjectCollection.Add(status);
                }
            }
        }

        /// <summary>
        /// 更新条目的收藏状态
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="collectionStatus"></param>
        public async Task UpdateCollectionStatus(SubjectBaseE subject, CollectionStatusType collectionStatus)
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (subject == null)
            {
                return;
            }
            // 由于服务器原因，导致条目在多个类别下出现，则有不属于同一类别的存在，则进行更新
            var cols = SubjectCollection.Where(sub => sub.Items.FirstOrDefault(it => it.SubjectId == subject.SubjectId) != null).ToList();
            if (cols.All(c => c.Status.Id == collectionStatus))
            {
                return;
            }
            var col = cols.FirstOrDefault(c => c.Status.Id != collectionStatus);
            IsUpdating = true;
            // 更新收藏状态成功后将条目从原类别中删除
            CollectionStatusE collectionStatusE = null;
            try
            {
                collectionStatusE = await BangumiApi.BgmApi.UpdateStatus(subject.SubjectId.ToString(), collectionStatus);
                NotificationHelper.Notify($"标记 {Converters.StringOneOrTwo(subject.Subject.NameCn, subject.Subject.Name)} {collectionStatusE.Status.Id.GetDesc(subject.Subject.Type)} 成功！");
            }
            catch (Exception e)
            {
                NotificationHelper.Notify($"标记 {Converters.StringOneOrTwo(subject.Subject.NameCn, subject.Subject.Name)} {collectionStatus.GetDesc(subject.Subject.Type)} 失败！\n" + e.Message,
                                          NotifyType.Error);
            }
            if (collectionStatusE is CollectionStatusE
                && col != null)
            {
                col.Items.Remove(subject);
                col.Count--;
                var index = SubjectCollection.IndexOf(col);
                SubjectCollection.Remove(col);
                if (col.Items.Count != 0)
                {
                    SubjectCollection.Insert(index, col);
                }
                // 找到新所属类别，有则加入，无则新增
                var newCol = SubjectCollection.FirstOrDefault(sub =>
                    sub.Status.Id == collectionStatusE.Status.Id);
                if (newCol != null)
                {
                    newCol.Items.Insert(0, subject);
                    newCol.Count++;
                    index = SubjectCollection.IndexOf(newCol);
                    SubjectCollection.Remove(newCol);
                    SubjectCollection.Insert(index, newCol);
                }
                else
                {
                    newCol = new Collection
                    {
                        Items = new List<SubjectBaseE> { subject },
                        Status = collectionStatusE.Status,
                        Count = 1
                    };
                    SubjectCollection.Add(newCol);
                }
            }
            IsUpdating = false;
        }


    }
}
