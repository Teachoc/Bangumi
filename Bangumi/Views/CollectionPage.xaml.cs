﻿using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.ViewModels;
using System;
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
    public sealed partial class CollectionPage : Page
    {
        public CollectionViewModel ViewModel { get; } = new CollectionViewModel();

        public CollectionPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.RootPage.RefreshButton.Click += CollectionPageRefresh;
            if (ViewModel.SubjectCollection.Count == 0 && !ViewModel.IsLoading)
            {
                ViewModel.LoadCollectionList();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainPage.RootPage.RefreshButton.Click -= CollectionPageRefresh;
        }

        private void CollectionPageRefresh(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button)
            {
                var tag = button.Tag;
                if (tag.Equals("收藏"))
                {
                    ViewModel.LoadCollectionList();
                }
            }
        }

        private void TypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.LoadCollectionList();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (SubjectBaseE)e.ClickedItem;
            MainPage.RootFrame.Navigate(typeof(DetailsPage), selectedItem.SubjectId, new DrillInNavigationTransitionInfo());
        }

        // 更新条目收藏状态
        private void UpdateCollectionStatusMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                var sub = item.DataContext as SubjectBaseE;
                switch (item.Tag)
                {
                    case "Wish":
                        ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.Wish);
                        break;
                    case "Collect":
                        ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.Collect);
                        break;
                    case "Doing":
                        ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.Do);
                        break;
                    case "OnHold":
                        ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.OnHold);
                        break;
                    case "Dropped":
                        ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.Dropped);
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
                SetMenuFlyoutByType();
                CollectionMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        // 触摸长按弹出菜单
        private void ItemRelativePanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin
                && !ViewModel.IsLoading
                && e.HoldingState == HoldingState.Started)
            {
                SetMenuFlyoutByType();
                CollectionMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        // 根据作品类别调整菜单文字
        private void SetMenuFlyoutByType()
        {
            switch (TypeCombobox.SelectedIndex)
            {
                case 1:
                    WishMenuFlyoutItem.Text = "想读";
                    CollectMenuFlyoutItem.Text = "读过";
                    DoingMenuFlyoutItem.Text = "在读";
                    break;
                case 2:
                    WishMenuFlyoutItem.Text = "想听";
                    CollectMenuFlyoutItem.Text = "听过";
                    DoingMenuFlyoutItem.Text = "在听";
                    break;
                case 3:
                    WishMenuFlyoutItem.Text = "想玩";
                    CollectMenuFlyoutItem.Text = "玩过";
                    DoingMenuFlyoutItem.Text = "在玩";
                    break;
                case 0:
                case 4:
                    WishMenuFlyoutItem.Text = "想看";
                    CollectMenuFlyoutItem.Text = "看过";
                    DoingMenuFlyoutItem.Text = "在看";
                    break;
                default:
                    WishMenuFlyoutItem.Text = "想做";
                    CollectMenuFlyoutItem.Text = "做过";
                    DoingMenuFlyoutItem.Text = "在做";
                    break;
            }
        }

    }
}
