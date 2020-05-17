using Bangumi.Api;
using Bangumi.BackgroundTasks;
using Bangumi.Common;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.ViewModels;
using Bangumi.Views;
using Microsoft.QueryStringDotNET;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Bangumi
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            UnhandledException += App_UnhandledException;
        }

        /// <summary>
        /// 未处理的异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            // 马上将缓存写入文件
            var task = BangumiApi.BgmCache.WriteToFile();
            Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog($"发生未知错误，应用即将关闭！\n{e.Message}", "未知错误");
            await dialog.ShowAsync();
            await task;
            Application.Current.Exit();
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }

        private async void OnLaunchedOrActivated(IActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            // 处理正常启动
            if (e is LaunchActivatedEventArgs launchActivatedArgs && launchActivatedArgs.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), launchActivatedArgs.Arguments);
                }
            }

            // Handle toast activation
            if (e is ToastNotificationActivatedEventArgs toastActivationArgs)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage));
                }

                // 等待加载完成
                while (true)
                {
                    if (MainPage.RootPage.IsLoaded)
                        break;
                    await Task.Delay(500);
                }

                // Parse the query string (using QueryString.NET)
                QueryString args = QueryString.Parse(toastActivationArgs.Argument);

                if (args.Contains("action"))
                {
                    string id = string.Empty;
                    // See what action is being requested 
                    switch (args["action"])
                    {
                        // Open the subject
                        case "viewSubject":
                            id = args["subjectId"];
                            MainPage.RootPage.ResetFrameBackStack();
                            MainPage.RootPage.NavigateToPage(typeof(EpisodePage), args["subjectId"], null);
                            break;
                        case "gotoPlaySite":
                            id = args["url"];
                            var sites = await BangumiData.GetAirSitesByBangumiIdAsync(id);
                            await Launcher.LaunchUriAsync(new Uri(args["url"]));
                            var episode = JsonConvert.DeserializeObject<EpisodeForSort>(args["episode"]);
                            ToastNotificationHelper.Toast("看完了吗？",
                                $"Ep.{episode.Sort} {Converters.StringOneOrTwo(episode.NameCn, episode.Name)}", "看完了！看完了！",
                                "markEpWatched", "episodeId", episode.Id.ToString(), string.Empty, string.Empty,
                                Microsoft.Toolkit.Uwp.Notifications.ToastActivationType.Background, true);
                            break;
                    }
                }
            }

            // 处理其它激活方式

            // 确保当前窗口处于活动状态
            Window.Current.Activate();
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            switch (args.TaskInstance.Task.Name)
            {
                case Constants.ToastBackgroundTask:
                    var activity = new ToastBackgroundTask();
                    activity.Run(args.TaskInstance);
                    break;
                case Constants.RefreshTokenTask:
                    BackgroundActivity.Start(args.TaskInstance);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // 在应用挂起或退出时马上将缓存写入文件
            await BangumiApi.BgmCache.WriteToFile();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}
