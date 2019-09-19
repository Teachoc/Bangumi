using Bangumi.Api;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.Views;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI;
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
        SystemNavigationManager systemNavigationManager;

        private bool _isOffline;
        public bool IsOffline
        {
            get => _isOffline;
            set
            {
                Set(ref _isOffline, value);
            }
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

            this.systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

            CostomTitleBar();


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
                            new FileHelper.EncryptionDelegate(EncryptionHelper.EncryptionAsync),
                            new FileHelper.DecryptionDelegate(EncryptionHelper.DecryptionAsync),
                            new BangumiApi.CheckNetworkDelegate(CheckNetworkStatus));

            if (SettingHelper.UseBangumiData)
            {
                // 初始化 BangumiData 对象
                _ = BangumiData.Init(Path.Combine(ApplicationData.Current.LocalFolder.Path, "bangumi-data"),
                                     SettingHelper.UseBiliApp);
            }
        }

        /// <summary>
        /// 检查网络状态
        /// </summary>
        /// <returns></returns>
        private bool CheckNetworkStatus()
        {
            IsOffline = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile() == null;
            return IsOffline;
        }

        /// <summary>
        /// 根据用户登录状态改变用户图标。
        /// 只检查 Token 是否存在。
        /// </summary>
        /// <returns></returns>
        private async Task UpdataUserStatusAsync()
        {
            bool result = await BangumiApi.CheckMyToken();
            if (result)
            {
                LoginButton.Label = "注销";
                UserIcon.Glyph = "\uE7E8";
                RootFrame.Navigate(typeof(HomePage));
            }
            else
            {
                LoginButton.Label = "登录";
                UserIcon.Glyph = "\uEE57";
                RootFrame.Navigate(typeof(LoginPage));
            }
        }

        /// <summary>
        /// 自定义标题栏
        /// </summary>
        private void CostomTitleBar()
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                // 将内容拓展到标题栏
                var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;
                // 设置标题栏按钮部分背景颜色
                var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
                appTitleBar.ButtonBackgroundColor = Colors.Transparent;
                appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                MainPage_ActualThemeChanged(null, "");
                ActualThemeChanged += MainPage_ActualThemeChanged;
            }
        }

        /// <summary>
        /// 在主题颜色改变时调用，设置标题栏按钮颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainPage_ActualThemeChanged(FrameworkElement sender, object args)
        {
            // 主题颜色改变时设置标题栏按钮颜色
            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (ActualTheme == ElementTheme.Light)
                appTitleBar.ButtonForegroundColor = Colors.Black;
            else
                appTitleBar.ButtonForegroundColor = Colors.White;
        }

        private void BackInvoked(KeyboardAccelerator sender,
                                 KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        /// <summary>
        /// 鼠标后退键返回上一页。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            var properties = args.CurrentPoint.Properties;

            // Ignore button chords with the left, right, and middle buttons
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed)
                return;

            // If back or foward are pressed (but not both) navigate appropriately
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                if (backPressed)
                    args.Handled = On_BackRequested();
                // if (forwardPressed) this.TryGoForward();
            }
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = On_BackRequested();
            }
        }

        // 页面向后导航
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 将 Esc 添加为后导航的键盘快捷键
            var goBack = new KeyboardAccelerator { Key = VirtualKey.Escape };
            goBack.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(goBack);

            // 删除Json缓存文件夹，v0.5.5 及之前版本，在从旧版本升级时使用
            if (System.IO.Directory.Exists(Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "JsonCache")))
                await (await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("JsonCache")).DeleteAsync();

            await UpdataUserStatusAsync();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            RootFrame.Navigate(typeof(SearchPage), null, new DrillInNavigationTransitionInfo());
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            RootFrame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
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
                await UpdataUserStatusAsync();
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
                    await UpdataUserStatusAsync();
                }
            }
        }

        /// <summary>
        /// 检查网络状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OfflineAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            BangumiApi.RecheckNetworkStatus();
        }
    }
}
