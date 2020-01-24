using Bangumi.Api;
using Bangumi.Data;
using Bangumi.Helper;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public string Version
        {
            get
            {
                var version = Package.Current.Id.Version;
                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.RootPage.MyCommandBar.Visibility = Visibility.Collapsed;

            EpsBatchToggleSwitch.IsOn = SettingHelper.EpsBatch;
            SubjectCompleteToggleSwitch.IsOn = SettingHelper.SubjectComplete;
            UseBangumiDataToggleSwitch.IsOn = SettingHelper.UseBangumiData;
            UseBangumiDataAirSitesToggleSwitch.IsOn = SettingHelper.UseBangumiDataAirSites;
            UseBilibiliUWPToggleSwitch.IsOn = SettingHelper.UseBiliApp;
            UseBangumiDataAirTimeToggleSwitch.IsOn = SettingHelper.UseBangumiDataAirTime;

            // 获取缓存文件大小
            JsonCacheSizeTextBlock.Text = ((double)BangumiApi.BgmCache.GetFileLength() / 1024).ToString("F3");
            DeleteUserCacheFileButton.IsEnabled = true;

            // 计算文件夹 ImageCache 中文件大小
            if (Directory.Exists(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "ImageCache")))
            {
                StorageFolder imageCacheFolder = await ApplicationData.Current.TemporaryFolder.GetFolderAsync("ImageCache");
                var files = await imageCacheFolder.GetFilesAsync();
                double fileSize = 0;
                foreach (var file in files)
                {
                    var fileInfo = await file.GetBasicPropertiesAsync();
                    fileSize += fileInfo.Size;
                }
                ImageCacheSizeTextBlock.Text = (fileSize / 1024).ToString("F3");
                DeleteImageTempFileButton.IsEnabled = true;
            }
            else
            {
                // 文件夹 ImageCache 不存在
                ImageCacheSizeTextBlock.Text = "0";
                DeleteImageTempFileButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// 设置开关
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SettingToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                switch (toggleSwitch.Tag.ToString())
                {
                    case "EpsBatch":
                        SettingHelper.EpsBatch = toggleSwitch.IsOn;
                        break;
                    case "SubjectComplete":
                        SettingHelper.SubjectComplete = toggleSwitch.IsOn;
                        break;
                    case "UseBangumiData":
                        SettingHelper.UseBangumiData = toggleSwitch.IsOn;
                        if(SettingHelper.UseBangumiData)
                        {
                            // 获取数据版本
                            BangumiData.Init(Path.Combine(ApplicationData.Current.LocalFolder.Path, "bangumi-data"),
                                             SettingHelper.UseBiliApp);
                            BangumiDataTextBlock.Text = "数据版本：" +
                                (string.IsNullOrEmpty(BangumiData.Version) ?
                                "无数据" :
                                BangumiData.Version);
                        }
                        break;
                    case "UseBangumiDataAirSites":
                        SettingHelper.UseBangumiDataAirSites = toggleSwitch.IsOn;
                        break;
                    case "UseBilibiliUWP":
                        SettingHelper.UseBiliApp = toggleSwitch.IsOn;
                        BangumiData.UseBiliApp = SettingHelper.UseBiliApp;
                        break;
                    case "UseBangumiDataAirTime":
                        SettingHelper.UseBangumiDataAirTime = toggleSwitch.IsOn;
                        break;
                }
            }
        }

        /// <summary>
        /// 检查更新并下载最新版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BangumiDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.IsEnabled = false;
                BangumiDataProgressRing.Visibility = Visibility.Visible;
                button.Content = "正在检查更新";
                var v = await BangumiData.GetLatestVersion();
                if (!string.IsNullOrEmpty(v))
                {
                    if (v != BangumiData.Version)
                    {
                        BangumiDataTextBlock.Text = "数据版本：" +
                            (string.IsNullOrEmpty(BangumiData.Version) ?
                            "无数据" :
                            BangumiData.Version) +
                            " -> " + v;
                        button.Content = "正在下载数据";
                        if (await BangumiData.DownloadLatestBangumiData())
                        {
                            BangumiDataTextBlock.Text = "数据版本：" +
                                (string.IsNullOrEmpty(BangumiData.Version) ?
                                "无数据" :
                                BangumiData.Version);
                            NotificationHelper.Notify("数据下载成功！");
                        }
                        else
                        {
                            NotificationHelper.Notify("数据下载失败，请重试或稍后再试！", NotificationHelper.NotifyType.Error);
                        }
                    }
                    else
                    {
                        NotificationHelper.Notify("已是最新版本！");
                    }
                }
                else
                {
                    NotificationHelper.Notify("获取最新版本失败！", NotificationHelper.NotifyType.Error);
                }
                button.Content = "检查更新";
                BangumiDataProgressRing.Visibility = Visibility.Collapsed;
                button.IsEnabled = true;
            }
        }

        private void DeleteJsonCacheFileButton_Click(object sender, RoutedEventArgs e)
        {
            // 删除缓存文件
            BangumiApi.BgmCache.Delete();
            JsonCacheSizeTextBlock.Text = "0";
            DeleteUserCacheFileButton.IsEnabled = false;
        }

        private async void DeleteImageTempFileButton_Click(object sender, RoutedEventArgs e)
        {
            // 删除图片缓存文件夹
            if (Directory.Exists(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "ImageCache")))
                await (await ApplicationData.Current.TemporaryFolder.GetFolderAsync("ImageCache")).DeleteAsync();
            ImageCacheSizeTextBlock.Text = "0";
            DeleteImageTempFileButton.IsEnabled = false;
        }
    }
}
