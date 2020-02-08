using Bangumi.Api;
using Bangumi.Common;
using Bangumi.Data;
using Bangumi.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
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
        };

        public string Version
        {
            get
            {
                var version = Package.Current.Id.Version;
                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

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
                    BangumiDataVersion = string.IsNullOrEmpty(BangumiData.Version) ?
                                         "无数据" :
                                         BangumiData.Version;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseBangumiDataAirSites));
                OnPropertyChanged(nameof(UseBiliApp));
                OnPropertyChanged(nameof(UseBangumiDataAirTime));
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
            set => Set(ref _bangumiDataStatus, value);
        }

        private bool _bangumiDataVersionChecking;
        public bool BangumiDataVersionChecking
        {
            get => _bangumiDataVersionChecking;
            set => Set(ref _bangumiDataVersionChecking, value);
        }

        private string _bangumiDataVersion;
        public string BangumiDataVersion
        {
            get
            {
                // 显示当前数据版本及更新后版本
                return string.IsNullOrEmpty(BangumiData.Version) ?
                       "无数据" :
                       (string.IsNullOrEmpty(_bangumiDataVersion) ?
                       BangumiData.Version :
                       $"{BangumiData.Version} -> {_bangumiDataVersion}");
            }
            set => Set(ref _bangumiDataVersion, value);
        }

        public async Task Load()
        {
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
        public async Task UpdateOrDownloadBangumiData()
        {
            BangumiDataStatus = "正在检查更新";
            BangumiDataVersionChecking = true;
            var v = await BangumiData.GetLatestVersion();
            if (!string.IsNullOrEmpty(v))
            {
                if (v != BangumiData.Version)
                {
                    BangumiDataVersion = v;
                    BangumiDataStatus = "正在下载数据";
                    if (await BangumiData.DownloadLatestBangumiData())
                    {
                        BangumiDataVersion = null;
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
