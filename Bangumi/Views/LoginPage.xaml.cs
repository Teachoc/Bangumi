﻿using Bangumi.Api;
using Bangumi.Helper;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e?.Parameter is string uri)
            {
                BitmapImage image = new BitmapImage { UriSource = new Uri(uri) };
                WelcomeImage.Source = image;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Wait, 10);
            try
            {
                await OAuthHelper.Authorize();
                if (BangumiApi.BgmOAuth.IsLogin)
                {
                    this.Frame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
                }
            }
            catch (Exception ex)
            {
                await NotifyControl.AddNotification($"登录失败，请重试！\n{ex.Message}", Controls.NotifyType.Error);
                //NotificationHelper.Notify($"登录失败，请重试！\n{ex.Message}", NotificationHelper.NotifyType.Error);
            }
            finally
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 10);
            }
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
        }

    }
}
