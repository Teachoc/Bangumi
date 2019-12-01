using Bangumi.Api;
using Bangumi.Api.Exceptions;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.Views;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Bangumi
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private bool _isOffline;
        public bool IsOffline
        {
            get => _isOffline;
            private set => Set(ref _isOffline, value);
        }
        public static MainPage RootPage;
        public static Frame RootFrame;
        public bool HasDialog = false;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名</param>
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 检查属性值是否相同。
        /// 仅在不同时设置属性值。
        /// </summary>
        /// <typeparam name="T">属性类型。</typeparam>
        /// <param name="storage">可读写的属性。</param>
        /// <param name="value">属性值。</param>
        /// <param name="propertyName">属性名。可被支持 CallerMemberName 的编译器自动提供。</param>
        /// <returns>值改变则返回 true，未改变返回 false。</returns>
        private bool Set<T>(ref T storage, T value,
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        public MainPage()
        {
            this.InitializeComponent();
            RootPage = this;
            RootFrame = MyFrame;
            // 设置窗口的最小大小
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(300, 200));

            // 初始化 Api 对象
            BangumiApi.Init(ApplicationData.Current.LocalFolder.Path,
                            ApplicationData.Current.LocalCacheFolder.Path,
                            "https://api.bgm.tv",
                            "https://bgm.tv/oauth",
                            // 将自己申请的应用相关信息填入
                            "bgm8905c514a1b94ec1", // ClientId
                            "b678c34dd896203627da308b6b453775", // ClientSecret
                            "BangumiGithubVersion", // RedirectUrl
                            "ms-appx:///Assets/resource/err_404.png",
                            EncryptionHelper.EncryptionAsync,
                            EncryptionHelper.DecryptionAsync,
                            new Func<bool>(() =>
                            {
                                // 检查网络状态
                                IsOffline = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile() == null;
                                return IsOffline;
                            }));

            if (SettingHelper.UseBangumiData)
            {
                // 初始化 BangumiData 对象
                _ = BangumiData.Init(Path.Combine(ApplicationData.Current.LocalFolder.Path, "bangumi-data"),
                                     SettingHelper.UseBiliApp);
            }
        }

        private void Page_Loaded(object s, RoutedEventArgs ev)
        {
            // 标题栏后退按钮
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) =>
            {
                if (!e.Handled)
                {
                    e.Handled = On_BackRequested();
                }
            };
            // 将 Esc 添加为后导航的键盘快捷键
            var goBack = new KeyboardAccelerator { Key = VirtualKey.Escape };
            goBack.Invoked += (sender, args) =>
            {
                On_BackRequested();
                args.Handled = true;
            };
            this.KeyboardAccelerators.Add(goBack);
            // 鼠标后退按钮
            Window.Current.CoreWindow.PointerPressed += (sender, args) =>
            {
                if (args.CurrentPoint.Properties.IsXButton1Pressed)
                {
                    args.Handled = On_BackRequested();
                }
            };

            SearchButton.Click += (sender, e) => RootFrame.Navigate(typeof(SearchPage), null, new DrillInNavigationTransitionInfo());
            SettingButton.Click += (sender, e) => RootFrame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
            OfflineAppBarButton.Click += (sender, e) => BangumiApi.RecheckNetworkStatus();

            _ = UpdateUserStatusAsync();
        }

        /// <summary>
        /// 页面向后导航
        /// </summary>
        /// <returns></returns>
        private bool On_BackRequested()
        {
            if (!MainPage.RootFrame.CanGoBack)
                return false;

            // 有弹出框时不向后导航
            if (HasDialog)
                return false;

            // 处于首页或登录页时不向后导航
            if (MainPage.RootFrame.CurrentSourcePageType == typeof(HomePage) ||
                MainPage.RootFrame.CurrentSourcePageType == typeof(LoginPage))
                return false;

            MainPage.RootFrame.GoBack();
            return true;
        }

        /// <summary>
        /// 点击登录按钮，根据登录状态执行不同操作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginButton.Label == "登录")
            {
                await UpdateUserStatusAsync();
            }
            else if (LoginButton.Label == "注销")
            {
                string choice = "";
                var msgDialog = new Windows.UI.Popups.MessageDialog("确定要退出登录吗？") { Title = "注销" };
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", uiCommand => { choice = uiCommand.Label; }));
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", uiCommand => { choice = uiCommand.Label; }));
                await msgDialog.ShowAsync();
                if (choice == "确定")
                {
                    BangumiApi.DeleteToken();
                    await UpdateUserStatusAsync();
                }
            }
        }

        /// <summary>
        /// 根据用户登录状态改变用户图标。
        /// 只检查 Token 是否存在。
        /// </summary>
        /// <returns></returns>
        private async Task UpdateUserStatusAsync()
        {
            try
            {
                var result = await BangumiApi.CheckMyToken();
                if (result.Item1)
                {
                    LoginButton.Label = "注销";
                    UserIcon.Glyph = "\uE7E8";
                    RootFrame.Navigate(typeof(HomePage), null, new DrillInNavigationTransitionInfo());
                }
                else
                {
                    LoginButton.Label = "登录";
                    UserIcon.Glyph = "\uEE57";
                    RootFrame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
                }
                await result.Item2;
            }
            catch (BgmUnauthorizedException)
            {
                // 授权过期，返回登录界面
                MainPage.RootFrame.Navigate(typeof(LoginPage), "ms-appx:///Assets/resource/err_401.png");
            }
        }

    }
}
