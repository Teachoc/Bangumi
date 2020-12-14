using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.ViewModels;
using System.Threading.Tasks;
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
    public sealed partial class CalendarPage : Page, IPageStatus
    {
        public CalendarViewModel ViewModel { get; } = new CalendarViewModel();

        public bool IsLoading => ViewModel.IsLoading;

        public async Task Refresh()
        {
            await ViewModel.PopulateCalendarAsync();
        }

        public CalendarPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsLoading)
            {
                if (ViewModel.CalendarCollection.Count == 0)
                {
                    ViewModel.PopulateCalendarFromCache();
                    ViewModel.PopulateCalendarAsync();
                }
                else
                {
                    ViewModel.PopulateCalendarFromCache();
                    if (ViewModel.CalendarCollection.Count == 0)
                    {
                        ViewModel.PopulateCalendarAsync();
                    }
                }
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (SubjectForCalendar)e.ClickedItem;
            this.Frame.Navigate(typeof(EpisodePage), selectedItem.Id, new DrillInNavigationTransitionInfo());
        }

        // 更新条目收藏状态
        private void UpdateCollectionStatusMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                switch (item.Tag)
                {
                    case "Wish":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForCalendar, CollectionStatusType.Wish);
                        break;
                    case "Collect":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForCalendar, CollectionStatusType.Collect);
                        break;
                    case "Doing":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForCalendar, CollectionStatusType.Do);
                        break;
                    case "OnHold":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForCalendar, CollectionStatusType.OnHold);
                        break;
                    case "Dropped":
                        ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForCalendar, CollectionStatusType.Dropped);
                        break;
                    default:
                        break;
                }
            }
        }

        // 鼠标右键弹出菜单
        private void GridView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin
                && !ViewModel.IsLoading
                && e.PointerDeviceType == PointerDeviceType.Mouse)
            {
                FrameworkElement element = e.OriginalSource switch
                {
                    GridViewItem item => item.ContentTemplateRoot as FrameworkElement,
                    FrameworkElement el => el,
                    _ => null
                };
                if (element != null && element.DataContext is SubjectForCalendar)
                {
                    e.Handled = true;
                    CollectionMenuFlyout.ShowAt(element, e.GetPosition(element));
                }
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
