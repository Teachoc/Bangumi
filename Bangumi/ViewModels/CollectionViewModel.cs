using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Api.Utils;
using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Views;
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

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => Set(ref _selectedIndex, value);
        }

        /// <summary>
        /// 刷新收藏列表，API限制每类最多25条。
        /// </summary>
        public async void LoadCollectionList()
        {
            try
            {
                var subjectType = GetSubjectType();
                if (BangumiApi.IsLogin)
                {
                    IsLoading = true;
                    await PopulateSubjectCollectionAsync(SubjectCollection, subjectType);
                }
                else
                {
                    //Message = "请先登录！";
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("获取用户收藏失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
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
        public async void UpdateCollectionStatus(Subject2 subject, CollectionStatusEnum collectionStatus)
        {
            if (subject != null)
            {
                // 由于服务器原因，导致条目在多个类别下出现，则有不属于同一类别的存在，则进行更新
                var cols = SubjectCollection.Where(sub => sub.Items.Where(it => it.SubjectId == subject.SubjectId).FirstOrDefault() != null);
                if (cols.Where(c => c.Status.Name != collectionStatus.GetDescCn((SubjectTypeEnum)subject.Subject.Type)).Count() == 0)
                    return;
                var col = cols.Where(c => c.Status.Name != collectionStatus.GetDescCn((SubjectTypeEnum)subject.Subject.Type)).FirstOrDefault();
                IsUpdating = true;
                if (await BangumiFacade.UpdateCollectionStatusAsync(subject.SubjectId.ToString(), collectionStatus))
                {
                    // 将条目从原类别中删除
                    col.Items.Remove(subject);
                    col.Count--;
                    var index = SubjectCollection.IndexOf(col);
                    SubjectCollection.Remove(col);
                    if (col.Items.Count != 0)
                        SubjectCollection.Insert(index, col);
                    // 找到新所属类别，有则加入，无则新增
                    var newCol = SubjectCollection.Where(sub => sub.Status.Name == collectionStatus.GetDescCn((SubjectTypeEnum)subject.Subject.Type)).FirstOrDefault();
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
                        newCol = new Collection()
                        {
                            Items = new List<Subject2>() { subject },
                            Status = new SubjectStatus() { Name = collectionStatus.GetDescCn((SubjectTypeEnum)subject.Subject.Type) },
                            Count = 1
                        };
                        SubjectCollection.Add(newCol);
                    }
                }
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 显示用户选定类别收藏信息。
        /// </summary>
        /// <param name="subjectCollection"></param>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        private async Task PopulateSubjectCollectionAsync(ObservableCollection<Collection> subjectCollection, SubjectTypeEnum subjectType)
        {
            try
            {
                // 获取缓存
                BangumiApi.BangumiCache.Collections.TryGetValue(subjectType.GetValue(), out Collection2 cache);
                if (cache != null && !cache.Collects.SequenceEqualExT(subjectCollection.ToList()))
                {
                    //清空原数据
                    subjectCollection.Clear();
                    foreach (var status in cache.Collects)
                    {
                        subjectCollection.Add(status);
                    }
                }

                var respose = await BangumiApi.GetSubjectCollectionAsync(subjectType);

                if (!cache.EqualsExT(respose))
                {
                    //清空原数据
                    subjectCollection.Clear();
                    foreach (var status in respose.Collects)
                    {
                        subjectCollection.Add(status);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("显示用户选定类别收藏信息失败。");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 根据下拉框返回所选择的收藏类型
        /// </summary>
        /// <returns></returns>
        private SubjectTypeEnum GetSubjectType()
        {
            switch (SelectedIndex)
            {
                case 0:
                    return SubjectTypeEnum.Anime;
                case 1:
                    return SubjectTypeEnum.Book;
                case 2:
                    return SubjectTypeEnum.Music;
                case 3:
                    return SubjectTypeEnum.Game;
                case 4:
                    return SubjectTypeEnum.Real;
                default:
                    return SubjectTypeEnum.Anime;
            }
        }



    }
}
