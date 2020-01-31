using Bangumi.Api;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page, INotifyPropertyChanged
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static HomePage homePage { get; private set; }

        public HomePage()
        {
            InitializeComponent();
            homePage = this;
#if DEBUG
            TitleBarEx.Text += " (Debug)";
#endif
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.RootPage.MyCommandBar.Visibility = Visibility.Visible;
            MainPage.RootPage.RefreshButton.IsEnabled = !IsLoading;
            MainPage.RootPage.MyCommandBar.IsDynamicOverflowEnabled = false;
            MainPage.RootPage.MyCommandBar.IsDynamicOverflowEnabled = true;

            if (BangumiApi.BgmOAuth.IsLogin)
            {
                if (HomePagePivot.Items.Cast<PivotItem>().All(p => p.Name != "CollectionItem"))
                {
                    HomePagePivot.Items.Insert(0, CollectionItem);
                }
                if (HomePagePivot.Items.Cast<PivotItem>().All(p => p.Name != "ProgressItem"))
                {
                    HomePagePivot.Items.Insert(0, ProgressItem);
                    HomePagePivot.SelectedIndex = 0;
                    ProgressPageFrame.Navigate(typeof(ProgressPage), null, new SuppressNavigationTransitionInfo());
                }
            }
            else
            {
                // 在进度页与收藏页任一页面存在时运行
                if (HomePagePivot.Items.Cast<PivotItem>().Any(p => p.Name == "ProgressItem")
                    || HomePagePivot.Items.Cast<PivotItem>().Any(p => p.Name == "CollectionItem"))
                {
                    HomePagePivot.Items.Remove(ProgressItem);
                    HomePagePivot.Items.Remove(CollectionItem);
                    TimeLinePageFrame.Navigate(typeof(TimeLinePage), null, new SuppressNavigationTransitionInfo());
                    MainPage.RootPage.RefreshButton.Tag = TimeLineItem.Header;
                }
            }
        }

        private void HomePagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            var pivotItem = pivot.SelectedItem as PivotItem;
            var frame = pivotItem.Content as Frame;
            MainPage.RootPage.RefreshButton.Tag = pivotItem.Header;
            if (frame.Content == null)
            {
                switch (pivotItem.Header)
                {
                    case "进度":
                        ProgressPageFrame.Navigate(typeof(ProgressPage), null, new SuppressNavigationTransitionInfo());
                        break;
                    case "收藏":
                        CollectionPageFrame.Navigate(typeof(CollectionPage), null, new SuppressNavigationTransitionInfo());
                        break;
                    case "时间表":
                        TimeLinePageFrame.Navigate(typeof(TimeLinePage), null, new SuppressNavigationTransitionInfo());
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
