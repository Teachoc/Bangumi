﻿using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using Bangumi.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ProgressPage : Page
    {
        public HomeViewModel ViewModel { get; } = new HomeViewModel();

        public ProgressPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ViewModel.watchingCollection.Count == 0 && !ViewModel.IsLoading)
            {
                ViewModel.LoadWatchingList();
            }
        }


        private void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            ViewModel.LoadWatchingList();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (WatchingStatus)e.ClickedItem;
            MainPage.rootFrame.Navigate(typeof(DetailsPage), selectedItem, new DrillInNavigationTransitionInfo());
            //Frame.Navigate(typeof(DetailsPage), selectedItem);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = MyGridView.ActualWidth - MyGridView.Padding.Left - MyGridView.Padding.Right;
            if (UseableWidth <= 0) return;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 235);
        }

        // 将下一话标记为看过
        private void NextEpButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Windows.UI.Xaml.Controls.Button)sender;
            var item = (WatchingStatus)button.DataContext;
            ViewModel.UpdateEpStatus(item);
        }
    }
}