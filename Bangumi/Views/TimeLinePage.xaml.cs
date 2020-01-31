using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.ViewModels;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TimeLinePage : Page
    {
        public TimeLineViewModel ViewModel { get; } = new TimeLineViewModel();

        public TimeLinePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.RootPage.RefreshButton.Click += TimeLinePageRefresh;
            if (ViewModel.TimeLineCollection.Count == 0 && !ViewModel.IsLoading)
            {
                ViewModel.LoadTimeLine();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainPage.RootPage.RefreshButton.Click -= TimeLinePageRefresh;
        }

        private void TimeLinePageRefresh(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button)
            {
                var tag = button.Tag;
                if (tag.Equals("时间表"))
                {
                    ViewModel.LoadTimeLine();
                }
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (SubjectForCalendar)e.ClickedItem;
            MainPage.RootFrame.Navigate(typeof(DetailsPage), selectedItem.Id, new DrillInNavigationTransitionInfo());
        }

        // 更新条目收藏状态
        private void UpdateCollectionStatusMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                switch (item.Tag)
                {
                    case "Wish":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectBase, CollectionStatusType.Wish);
                        break;
                    case "Collect":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectBase, CollectionStatusType.Collect);
                        break;
                    case "Doing":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectBase, CollectionStatusType.Do);
                        break;
                    case "OnHold":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectBase, CollectionStatusType.OnHold);
                        break;
                    case "Dropped":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectBase, CollectionStatusType.Dropped);
                        break;
                    default:
                        break;
                }
            }
        }

        // 鼠标右键弹出菜单
        private void ItemRelativePanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin
                && !ViewModel.IsLoading
                && e.PointerDeviceType == PointerDeviceType.Mouse)
            {
                CollectionMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        // 触摸长按弹出菜单
        private void RelativePanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin
                && !ViewModel.IsLoading
                && e.HoldingState == HoldingState.Started)
            {
                CollectionMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }
    }
}
