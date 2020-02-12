using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SplitButton = Microsoft.UI.Xaml.Controls.SplitButton;
using SplitButtonClickEventArgs = Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DetailsPage : Page, IPageStatus
    {
        public DetailsViewModel ViewModel { get; private set; } = new DetailsViewModel();

        public bool IsLoading => ViewModel.IsLoading;

        public async Task Refresh()
        {
            await ViewModel.LoadDetails();
        }

        public DetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            (((Frame.Parent as Microsoft.UI.Xaml.Controls.NavigationView)?.Parent as Grid).Parent as MainPage)?.SelectPlaceholderItem("详情");

            if (e.Parameter?.GetType() == typeof(int))
            {
                ViewModel.SubjectId = e.Parameter.ToString();
            }
            else
            {
                return;
            }

            ViewModel.InitViewModel();
            ViewModel.LoadDetails();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingHelper.UseBangumiDataAirSites)
            {
                InitAirSites();
            }
            else
            {
                SitesMenuFlyout.Items?.Clear();
                SelectedTextBlock.Text = "";
                SelectedTextBlock.DataContext = null;
            }
        }

        /// <summary>
        /// 修改章节状态。
        /// </summary>
        private void UpdateEpStatusMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                var ep = item.DataContext as Episode;
                switch (item.Tag)
                {
                    case "Watched":
                        ViewModel.UpdateEpStatus(ep, EpStatusType.watched);
                        break;
                    case "WatchedTo":
                        ViewModel.UpdateEpStatusBatch(ep);
                        break;
                    case "Queue":
                        ViewModel.UpdateEpStatus(ep, EpStatusType.queue);
                        break;
                    case "Drop":
                        ViewModel.UpdateEpStatus(ep, EpStatusType.drop);
                        break;
                    case "Remove":
                        ViewModel.UpdateEpStatus(ep, EpStatusType.remove);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 修改章节状态弹出菜单。
        /// </summary>
        private void Eps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin && !ViewModel.IsProgressLoading && ((sender as RelativePanel)?.DataContext as Episode)?.Status != "NA")
            {
                EpMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        /// <summary>
        /// 鼠标右键修改章节状态弹出菜单，无视章节状态。
        /// </summary>
        private void Eps_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin
                && !ViewModel.IsProgressLoading
                && e.PointerDeviceType == PointerDeviceType.Mouse)
            {
                EpMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        /// <summary>
        /// 触摸长按修改章节状态弹出菜单，无视章节状态。
        /// </summary>
        private void Eps_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin
                && !ViewModel.IsProgressLoading
                && e.HoldingState == HoldingState.Started)
            {
                EpMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        private async void LaunchWebPage_Click(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            var uriWebPage = new Uri("https://bgm.tv/subject/" + ViewModel.SubjectId);

            // Launch the URI
            await Launcher.LaunchUriAsync(uriWebPage);
        }

        private async void ItemsRepeater_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is RelativePanel panel)
            {
                // The URI to launch
                var uriWebPage = new Uri(panel.DataContext.ToString());

                // Launch the URI
                await Launcher.LaunchUriAsync(uriWebPage);
            }
        }

        private void RelativePanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is RelativePanel panel)
            {
                panel.Background = Resources["ListViewItemBackgroundPointerOver"] as SolidColorBrush;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 10);
            }
        }

        private void RelativePanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is RelativePanel panel)
            {
                panel.Background = Resources["ListViewItemBackgroundPressed"] as SolidColorBrush;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 10);
            }
        }

        private void RelativePanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is RelativePanel panel)
            {
                panel.Background = Converters.ConvertBrushFromString("Transparent");
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 10);
            }
        }

        /// <summary>
        /// 点击拆分按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void SitesSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            if (sender.Content is TextBlock textBlock)
            {
                var uri = textBlock.DataContext as string;
                await Launcher.LaunchUriAsync(new Uri(uri));
            }
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
                SelectedTextBlock.Text = item.Text;
                SelectedTextBlock.DataContext = uri;
                await Launcher.LaunchUriAsync(new Uri(uri));
            }
        }

        /// <summary>
        /// 初始化放送站点及拆分按钮
        /// </summary>
        private async Task InitAirSites()
        {
            SitesMenuFlyout.Items.Clear();
            SelectedTextBlock.Text = "";
            SelectedTextBlock.DataContext = null;
            var airSites = await BangumiData.GetAirSitesByBangumiIdAsync(ViewModel.SubjectId);
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
                SelectedTextBlock.Text = airSites[0].SiteName;
                SelectedTextBlock.DataContext = airSites[0].Url;
            }
        }
    }
}
