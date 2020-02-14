using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
    public sealed partial class EpisodePage : Page, IPageStatus
    {
        public EpisodeViewModel ViewModel { get; private set; } = new EpisodeViewModel();

        public bool IsLoading => ViewModel.IsLoading;

        public async Task Refresh()
        {
            await ViewModel.LoadDetails();
        }

        public EpisodePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            (((Frame.Parent as Microsoft.UI.Xaml.Controls.NavigationView)?.Parent as Grid).Parent as MainPage)?.SelectPlaceholderItem("章节");

            if (e.Parameter?.GetType() == typeof(int) &&
                (e.NavigationMode != NavigationMode.Back || ViewModel.SubjectId != e.Parameter.ToString()))
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

        private async void LaunchWebPage_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"https://bgm.tv/subject/{ViewModel.SubjectId}"));
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

        /// <summary>
        /// 根据章节状态填充按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EpFlyout_Opened(object sender, object e)
        {
            if (!(sender is Flyout flyout))
            {
                return;
            }
            if ((flyout.Content as Panel)?.Children[2] is Panel panel)
            {
                if (!BangumiApi.BgmOAuth.IsLogin)
                {
                    panel.Visibility = Visibility.Collapsed;
                    return;
                }
                panel.Visibility = Visibility.Visible;
                panel.Children.Clear();
                var buttons = new List<(string, string, string, bool)>();
                if (flyout.Target.DataContext is EpisodeWithEpStatus episode)
                {
                    switch (episode.EpStatus)
                    {
                        case EpStatusType.watched:
                            buttons.Add(("看过", "EpWatchedBackground", "Watched", true));
                            buttons.Add(("想看", "EpQueueBackground", "Queue", false));
                            buttons.Add(("抛弃", "EpDropBackground", "Drop", false));
                            buttons.Add(("撤销", "EpBackground", "Remove", false));
                            break;
                        case EpStatusType.queue:
                            buttons.Add(("看过", "EpWatchedBackground", "Watched", false));
                            buttons.Add(("想看", "EpQueueBackground", "Queue", true));
                            buttons.Add(("抛弃", "EpDropBackground", "Drop", false));
                            buttons.Add(("撤销", "EpBackground", "Remove", false));
                            break;
                        case EpStatusType.drop:
                            buttons.Add(("看过", "EpWatchedBackground", "Watched", false));
                            buttons.Add(("想看", "EpQueueBackground", "Queue", false));
                            buttons.Add(("抛弃", "EpDropBackground", "Drop", true));
                            buttons.Add(("撤销", "EpBackground", "Remove", false));
                            break;
                        default:
                            buttons.Add(("看过", "EpWatchedBackground", "Watched", false));
                            buttons.Add(("看到", "EpWatchedBackground", "WatchedTo", false));
                            buttons.Add(("想看", "EpQueueBackground", "Queue", false));
                            buttons.Add(("抛弃", "EpDropBackground", "Drop", false));
                            break;
                    }
                }
                foreach (var item in buttons)
                {
                    var button = new RadioButton
                    {
                        Style = (Style)Application.Current.Resources["FilledRadioButtonStyle"],
                        Content = item.Item1,
                        Background = (SolidColorBrush)Application.Current.Resources[item.Item2],
                        CornerRadius = new CornerRadius(5),
                        Tag = item.Item3,
                        IsChecked = item.Item4
                    };
                    button.Checked += EpStatusRadioButton_Checked;
                    panel.Children.Add(button);
                }
            }
        }

        private void EpStatusRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton item)
            {
                var ep = item.DataContext as EpisodeWithEpStatus;
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

        private void NavigateToDetailPage_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DetailPage), ViewModel.Detail);
        }
    }
}
