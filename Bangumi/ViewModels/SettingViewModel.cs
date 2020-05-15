using Bangumi.Api;
using Bangumi.Common;
using Bangumi.Controls;
using Bangumi.Data;
using Bangumi.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Bangumi.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        public List<Url> Urls { get; set; } = new List<Url>
        {
            new Url {Uri="https://github.com/bangumi/api", Desc="Bangumi API(https://github.com/bangumi/api)"},
            new Url {Uri="https://www.newtonsoft.com/json", Desc="Newtonsoft.Json(https://www.newtonsoft.com/json)"},
            new Url {Uri="https://github.com/Microsoft/microsoft-ui-xaml", Desc="Microsoft.UI.Xaml(https://github.com/Microsoft/microsoft-ui-xaml)"},
            new Url {Uri="https://github.com/windows-toolkit/WindowsCommunityToolkit", Desc="WindowsCommunityToolkit(https://github.com/windows-toolkit/WindowsCommunityToolkit)"},
            new Url {Uri="https://github.com/bangumi-data/bangumi-data", Desc="bangumi-data(https://github.com/bangumi-data/bangumi-data)"},
            new Url {Uri="https://flurl.dev", Desc="Flurl(https://flurl.dev)"},
            new Url {Uri="https://github.com/Tlaster/WeiPo", Desc="WeiPo(https://github.com/Tlaster/WeiPo)"},
            new Url {Uri="https://github.com/tobiichiamane/pixivfs-uwp", Desc="pixivfs-uwp(https://github.com/tobiichiamane/pixivfs-uwp)"},
        };

        public string Version => string.Format("版本：{0} v{1}.{2}.{3}.{4} {5}",
                                    Package.Current.DisplayName,
                                    Package.Current.Id.Version.Major,
                                    Package.Current.Id.Version.Minor,
                                    Package.Current.Id.Version.Build,
                                    Package.Current.Id.Version.Revision,
                                    Package.Current.Id.Architecture);

        public string PackageName => string.Format("包名：{0}", Package.Current.Id.Name);
        public string InstalledDate => string.Format("安装时间：{0}", Package.Current.InstalledDate.ToLocalTime().DateTime);

        public bool EpsBatch
        {
            get => SettingHelper.EpsBatch;
            set
            {
                SettingHelper.EpsBatch = value;
                OnPropertyChanged();
            }
        }

        public bool SubjectComplete
        {
            get => SettingHelper.SubjectComplete;
            set
            {
                SettingHelper.SubjectComplete = value;
                OnPropertyChanged();
            }
        }

        public bool OrderByAirTime
        {
            get => SettingHelper.OrderByAirTime;
            set
            {
                SettingHelper.OrderByAirTime = value;
                OnPropertyChanged();
            }
        }

        public bool EnableBackgroundTask
        {
            get
            {
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    if (cur.Value.Name == "RefreshTokenTask")
                    {
                        return true;
                    }
                }
                return false;
            }
            set
            {
                if (value)
                {
                    // If the user denies access, the task will not run.
                    var requestTask = BackgroundExecutionManager.RequestAccessAsync();

                    var builder = new BackgroundTaskBuilder();

                    builder.Name = "RefreshTokenTask";

                    builder.SetTrigger(new TimeTrigger((uint)TimeSpan.FromDays(1).TotalMinutes, false));

                    builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

                    // If the condition changes while the background task is executing then it will
                    // be canceled.
                    builder.CancelOnConditionLoss = true;

                    builder.Register();

                }
                else
                {
                    EnableBangumiAirToast = false;
                    foreach (var cur in BackgroundTaskRegistration.AllTasks)
                    {
                        if (cur.Value.Name == "RefreshTokenTask")
                        {
                            cur.Value.Unregister(true);
                        }
                    }
                }

                OnPropertyChanged();
            }
        }

        public bool UseBangumiData
        {
            get => SettingHelper.UseBangumiData;
            set
            {
                SettingHelper.UseBangumiData = value;
                if (value)
                {
                    // 获取数据版本
                    BangumiData.Init(Path.Combine(ApplicationData.Current.LocalFolder.Path, "bangumi-data"),
                                     SettingHelper.UseBiliApp);
                }
                else
                {
                    BangumiData.AutoCheck = false;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(BangumiDataVersion));
                OnPropertyChanged(nameof(UseBangumiDataAirSites));
                OnPropertyChanged(nameof(UseBiliApp));
                OnPropertyChanged(nameof(UseBangumiDataAirTime));
                OnPropertyChanged(nameof(BangumiDataAutoCheck));
                OnPropertyChanged(nameof(BangumiDataCheckInterval));
                OnPropertyChanged(nameof(BangumiDataAutoUpdate));
            }
        }

        public bool BangumiDataAutoCheck
        {
            get => BangumiData.AutoCheck;
            set
            {
                BangumiData.AutoCheck = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BangumiDataCheckInterval));
                OnPropertyChanged(nameof(BangumiDataAutoUpdate));
            }
        }

        public int BangumiDataCheckInterval
        {
            get => BangumiData.CheckInterval;
            set
            {
                BangumiData.CheckInterval = value;
                OnPropertyChanged();
            }
        }

        public bool BangumiDataAutoUpdate
        {
            get => BangumiData.AutoUpdate;
            set
            {
                BangumiData.AutoUpdate = value;
                OnPropertyChanged();
            }
        }


        public bool UseBangumiDataAirSites
        {
            get => SettingHelper.UseBangumiDataAirSites;
            set
            {
                SettingHelper.UseBangumiDataAirSites = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseBiliApp));
            }
        }

        public bool UseBiliApp
        {
            get => SettingHelper.UseBiliApp;
            set
            {
                SettingHelper.UseBiliApp = value;
                BangumiData.UseBiliApp = value;
                OnPropertyChanged();
            }
        }

        public bool UseBangumiDataAirTime
        {
            get => SettingHelper.UseBangumiDataAirTime;
            set
            {
                SettingHelper.UseBangumiDataAirTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnableBangumiAirToast));
            }
        }

        public bool EnableBangumiAirToast
        {
            get => SettingHelper.EnableBangumiAirToast;
            set
            {
                if (value)
                {
                    if (!EnableBackgroundTask)
                    {
                        NotificationHelper.Notify("该选项需启用后台任务后可用", NotifyType.Warn);
                        value = false;
                    }
                }
                else
                {
                    ToastNotificationHelper.RemoveAllScheduledToasts();
                }
                SettingHelper.EnableBangumiAirToast = value;
                OnPropertyChanged();
            }
        }

        private bool _userCacheCanDelete;
        public bool UserCacheCanDelete
        {
            get => _userCacheCanDelete;
            set => Set(ref _userCacheCanDelete, value);
        }

        private string _userCacheSize;
        public string UserCacheSize
        {
            get => _userCacheSize;
            set => Set(ref _userCacheSize, value);
        }

        private bool _imageCacheCanDelete;
        public bool ImageCacheCanDelete
        {
            get => _imageCacheCanDelete;
            set => Set(ref _imageCacheCanDelete, value);
        }

        private string _imageCacheSize;
        public string ImageCacheSize
        {
            get => _imageCacheSize;
            set => Set(ref _imageCacheSize, value);
        }

        private string _bangumiDataStatus = "检查更新";
        public string BangumiDataStatus
        {
            get => _bangumiDataStatus;
            set
            {
                Set(ref _bangumiDataStatus, value);
                OnPropertyChanged(nameof(BangumiDataVersion));
                OnPropertyChanged(nameof(LastUpdate));
            }
        }

        private bool _bangumiDataVersionChecking;
        public bool BangumiDataVersionChecking
        {
            get => _bangumiDataVersionChecking;
            set => Set(ref _bangumiDataVersionChecking, value);
        }

        public string BangumiDataVersion
        {
            get
            {
                // 显示当前数据版本及更新后版本
                var version = string.IsNullOrEmpty(BangumiData.Version) ?
                       "无数据" : BangumiData.Version;
                version += ((string.IsNullOrEmpty(BangumiData.LatestVersion) || BangumiData.Version == BangumiData.LatestVersion) ?
                       string.Empty :
                       $" -> {BangumiData.LatestVersion}");
                return version;
            }
        }

        public string LastUpdate
        {
            get
            {
                if (BangumiData.LastUpdate == DateTimeOffset.MinValue)
                {
                    return "从未更新";
                }
                return BangumiData.LastUpdate.ToString("yyyy-MM-dd");
            }
        }

        public async Task Load()
        {
            if (UserCacheSize == "正在计算")
            {
                return;
            }
            ImageCacheCanDelete = false;
            ImageCacheSize = "正在计算";
            UserCacheCanDelete = false;
            UserCacheSize = "正在计算";

            // 获取缓存文件大小
            UserCacheSize = FileSizeHelper.GetSizeString(BangumiApi.BgmCache.GetFileLength());
            UserCacheCanDelete = true;

            // 计算文件夹 ImageCache 中文件大小
            if (Directory.Exists(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "ImageCache")))
            {
                StorageFolder imageCacheFolder = await ApplicationData.Current.TemporaryFolder.GetFolderAsync("ImageCache");
                var files = await imageCacheFolder.GetFilesAsync();
                ulong fileSize = 0;
                foreach (var file in files)
                {
                    var fileInfo = await file.GetBasicPropertiesAsync();
                    fileSize += fileInfo.Size;
                }
                ImageCacheSize = FileSizeHelper.GetSizeString(fileSize);
                ImageCacheCanDelete = true;
            }
            else
            {
                // 文件夹 ImageCache 不存在
                ImageCacheSize = "无缓存";
            }
        }

        public void DeleteUserCacheFile()
        {
            // 删除缓存文件
            BangumiApi.BgmCache.Delete();
            UserCacheSize = "无缓存";
            UserCacheCanDelete = false;
        }

        /// <summary>
        /// 删除图片缓存文件夹
        /// </summary>
        public async Task DeleteImageTempFile()
        {
            ImageCacheCanDelete = false;
            if (Directory.Exists(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "ImageCache")))
            {
                await (await ApplicationData.Current.TemporaryFolder.GetFolderAsync("ImageCache")).DeleteAsync();
            }
            ImageCacheSize = "无缓存";
        }

        /// <summary>
        /// 检查更新并下载最新版本
        /// </summary>
        public async Task UpdateBangumiData()
        {
            BangumiDataStatus = "正在检查更新";
            BangumiDataVersionChecking = true;
            bool hasNew = false;
            var startDownloadAction = new Action(() =>
            {
                hasNew = true;
                BangumiDataStatus = "正在下载数据";
            });
            if (await BangumiData.DownloadLatestBangumiData(startDownloadAction))
            {
                if (hasNew)
                {
                    NotificationHelper.Notify("数据下载成功！");
                }
                else
                {
                    NotificationHelper.Notify("已是最新版本！");
                }
            }
            else
            {
                if (hasNew)
                {
                    NotificationHelper.Notify("数据下载失败，请重试或稍后再试！", NotifyType.Error);
                }
                else
                {
                    NotificationHelper.Notify("获取最新版本失败！", NotifyType.Error);
                }
            }
            BangumiDataStatus = "检查更新";
            BangumiDataVersionChecking = false;
        }


    }

    public class Url
    {
        public string Uri { get; set; }
        public string Desc { get; set; }
    }
}
