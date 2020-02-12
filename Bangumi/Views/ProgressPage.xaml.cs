using Bangumi.Api;
using Bangumi.Api.Exceptions;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
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
    public sealed partial class ProgressPage : Page, IPageStatus
    {
        public ProgressViewModel ViewModel { get; } = new ProgressViewModel();

        public bool IsLoading => ViewModel.IsLoading;

        public async Task Refresh()
        {
            await ViewModel.PopulateWatchingListAsync();
        }

        public ProgressPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!BangumiApi.BgmOAuth.IsLogin)
            {
                MainPage.RootPage.Frame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
                return;
            }
            if (!ViewModel.IsLoading)
            {
                if (ViewModel.WatchingCollection.Count == 0)
                {
                    ViewModel.PopulateWatchingListFromCache();
                    if (!BangumiApi.BgmCache.IsUpdatedToday)
                    {
                        try
                        {
                            await BangumiApi.BgmOAuth.CheckToken();
                        }
                        catch (BgmUnauthorizedException)
                        {
                            // 授权过期，返回登录界面
                            (Window.Current.Content as Frame).Navigate(typeof(LoginPage), "ms-appx:///Assets/resource/err_401.png");
                        }
                    }
                    await ViewModel.PopulateWatchingListAsync();
                }
                else
                {
                    ViewModel.PopulateWatchingListFromCache();
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private async void ProgressPageRefresh(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button)
            {
                var tag = button.Tag;
                if (tag.Equals("progress"))
                {
                    await ViewModel.PopulateWatchingListAsync();
                }
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (WatchStatus)e.ClickedItem;
            this.Frame.Navigate(typeof(DetailsPage), selectedItem.SubjectId, new DrillInNavigationTransitionInfo());
        }

        /// <summary>
        /// 将下一话标记为看过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextEpButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var item = (WatchStatus)button.DataContext;
            ViewModel.UpdateNextEpStatus(item);
        }

        /// <summary>
        /// 修改收藏状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var item = (WatchStatus)button.DataContext;
            ViewModel.EditCollectionStatus(item);
        }

        /// <summary>
        /// 点击菜单项打开站点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SiteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                var uri = item.DataContext as string;
                await Launcher.LaunchUriAsync(new Uri(uri));
            }
        }

        /// <summary>
        /// 初始化放送站点及拆分按钮
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task InitAirSites(string id)
        {
            SitesMenuFlyout.Items.Clear();
            var airSites = await BangumiData.GetAirSitesByBangumiIdAsync(id);
            if (airSites.Count != 0)
            {
                foreach (var site in airSites)
                {
                    MenuFlyoutItem menuFlyoutItem = new MenuFlyoutItem
                    {
                        Text = site.SiteName,
                        DataContext = site.Url
                    };
                    menuFlyoutItem.Click += SiteMenuFlyoutItem_Click;
                    SitesMenuFlyout.Items.Add(menuFlyoutItem);
                }
            }
            else
            {
                MenuFlyoutItem menuFlyoutItem = new MenuFlyoutItem
                {
                    Text = "无放送站点"
                };
                SitesMenuFlyout.Items.Add(menuFlyoutItem);
            }
        }

        /// <summary>
        /// 显示右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="point"></param>
        private async Task ShowSitesMenuFlyout(FrameworkElement sender, Point point)
        {
            if (sender is RelativePanel panel && panel.DataContext is WatchStatus watch)
            {
                await InitAirSites(watch.SubjectId.ToString());
                SitesMenuFlyout.ShowAt(sender, point);
            }
        }

        /// <summary>
        /// 鼠标右键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RelativePanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (SettingHelper.UseBangumiDataAirSites && e.PointerDeviceType == PointerDeviceType.Mouse)
            {
                e.Handled = true;
                ShowSitesMenuFlyout((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        /// <summary>
        /// 触摸长按
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RelativePanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (SettingHelper.UseBangumiDataAirSites && e.HoldingState == HoldingState.Started)
            {
                e.Handled = true;
                ShowSitesMenuFlyout((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }
    }
}
