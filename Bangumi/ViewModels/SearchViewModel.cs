using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Bangumi.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        public ObservableCollection<string> Suggestions { get; private set; } = new ObservableCollection<string>();
        public SearchResultIncrementalLoadingCollection SearchResultCollection;

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set => Set(ref _isUpdating, value);
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => Set(ref _selectedIndex, value);
        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public ObservableCollection<string> SearchStatus { get; private set; } = new ObservableCollection<string>
        {
            "","","","","",""
        };

        public ObservableCollection<string> ResultNumber { get; private set; } = new ObservableCollection<string>
        {
            "","","","","",""
        };

        public ObservableCollection<bool> NoResult { get; private set; } = new ObservableCollection<bool>
        {
            false,false,false,false,false,false
        };


        public string[] PreSearch = new string[6];

        /// <summary>
        /// 获取搜索建议。
        /// </summary>
        public async void GetSearchSuggestions()
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                try
                {
                    Debug.WriteLine("开始获取搜索建议");
                    var result = await BangumiApi.GetSearchResultAsync(SearchText, "", 0, 10);
                    if (SearchText == PreSearch[SelectedIndex])
                    {
                        return;
                    }
                    Suggestions.Clear();
                    foreach (var item in result.Results)
                    {
                        Suggestions.Add(item.NameCn);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("获取搜索建议失败！\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// 重置搜索页结果状态显示
        /// </summary>
        public void ResetSearchStatus()
        {
            for (int i = 0; i < ResultNumber.Count; i++)
            {
                ResultNumber[i] = "";
                SearchStatus[i] = "";
                NoResult[i] = false;
                PreSearch[i] = "";
            }
        }

        /// <summary>
        /// 检查当前关键词是否与上一次搜索相同。
        /// </summary>
        /// <returns></returns>
        public bool CheckIfSearched()
        {
            if (SearchText == PreSearch[SelectedIndex])
            {
                return true;
            }
            else
            {
                PreSearch[SelectedIndex] = SearchText;
                return false;
            }
        }

        /// <summary>
        /// 开始加载。
        /// </summary>
        /// <param name="index">当前搜索所属类型所在的Pivot索引。</param>
        public void OnLoadMoreStarted(int index)
        {
            SearchStatus[index] = "正在加载...";
        }

        /// <summary>
        /// 加载完成。
        /// </summary>
        /// <param name="index">当前搜索所属类型所在的Pivot索引。</param>
        /// <param name="itemsCount"></param>
        /// <param name="hasMoreItems"></param>
        public void OnLoadMoreCompleted(int index, int itemsCount, bool hasMoreItems)
        {
            ResultNumber[index] = "(" + itemsCount + ")";
            if (!hasMoreItems)
            {
                if (itemsCount == 0)
                {
                    SearchStatus[index] = "";
                    ResultNumber[index] = "";
                    NoResult[index] = true;
                }
                else
                {
                    SearchStatus[index] = "没有更多了";
                }
            }
        }

        /// <summary>
        /// 更新条目的收藏状态
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="collectionStatus"></param>
        public async void UpdateCollectionStatus(Subject subject, CollectionStatusEnum collectionStatus)
        {
            if (subject != null)
            {
                IsUpdating = true;
                await BangumiFacade.UpdateCollectionStatusAsync(subject.Id.ToString(), collectionStatus);
                IsUpdating = false;
            }
        }

    }

    /// <summary>
    /// 在页面到达底部时加载更多。
    /// </summary>
    public class SearchResultIncrementalLoadingCollection : ObservableCollection<Subject>, ISupportIncrementalLoading
    {
        int offset = 0;
        int max = 20;
        int index;
        int itemsCount = 0;
        private string keyword;
        private string type;
        bool isSearching = false;

        public SearchResultIncrementalLoadingCollection(string keyword, string type, int index)
        {
            this.keyword = keyword;
            this.type = type;
            this.index = index;
        }

        public bool HasMoreItems => offset < max;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var dispatcher = Window.Current.Dispatcher;
            return AsyncInfo.Run(async cancelToken =>
            {
                if (isSearching)
                    return new LoadMoreItemsResult { Count = count };
                await Task.WhenAll(dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        isSearching = true;
                        Debug.WriteLine("Loading {0}/{1} items ({2})", offset + 20, max, type);
                        // 加载开始事件
                        if (this.OnLoadMoreStarted != null)
                        {
                            this.OnLoadMoreStarted(index);
                        }
                        SearchResult result = await BangumiApi.GetSearchResultAsync(keyword, type, offset, 20);
                        max = result.ResultCount;
                        foreach (Subject item in result.Results)
                        {
                            Add(item);
                        }
                        if (!HasMoreItems)
                        {
                            System.Diagnostics.Debug.WriteLine("Loading complete.");
                        }
                        itemsCount += result.Results.Count;
                        offset += 20;

                    }
                    catch (Exception e)
                    {
                        NotificationHelper.Notify("获取搜索结果失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                                  NotificationHelper.NotifyType.Error);
                        offset = max;
                    }
                    finally
                    {
                        // 加载完成事件
                        if (this.OnLoadMoreCompleted != null)
                        {
                            if (offset > max)
                            {
                                offset = max;
                            }
                            this.OnLoadMoreCompleted(index, itemsCount, HasMoreItems);
                        }
                        isSearching = false;
                    }
                }).AsTask());

                return new LoadMoreItemsResult { Count = count };
            });
        }
        public delegate void LoadMoreStarted(int index);
        public delegate void LoadMoreCompleted(int index, int itemsCount, bool hasMoreItems);

        public event LoadMoreStarted OnLoadMoreStarted;
        public event LoadMoreCompleted OnLoadMoreCompleted;
    }
}
