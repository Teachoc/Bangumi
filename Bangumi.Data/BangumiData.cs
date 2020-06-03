using Bangumi.Data.Models;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bangumi.Data
{
    public static class BangumiData
    {
        private const string BangumiDataUrl = "https://api.github.com/repos/bangumi-data/bangumi-data/tags";
        private const string BangumiDataCDNUrl = "https://cdn.jsdelivr.net/npm/bangumi-data@0.3/dist/data.json";
        private static BangumiDataSet _dataSet;
        private static Dictionary<string, string> _seasonIdMap;
        private static VersionInfo _info = new VersionInfo();
        private static string _folderPath;
        public static string LatestVersion { get; private set; }
        public static string Version => _info.Version;
        public static DateTimeOffset LastUpdate => _info.LastUpdate;
        private static bool _useBiliApp;
        public static bool UseBiliApp
        {
            get => _useBiliApp;
            set
            {
                _useBiliApp = value;
                if (value)
                {
                    if (File.Exists(AppFile.Map_json.GetFilePath(_folderPath)))
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                _seasonIdMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(await FileHelper.ReadTextAsync(AppFile.Map_json.GetFilePath(_folderPath)));
                            }
                            catch
                            {
                                FileHelper.DeleteFile(AppFile.Map_json.GetFilePath(_folderPath));
                            }
                        }).Wait();
                    }
                    _seasonIdMap ??= new Dictionary<string, string>();
                }
            }
        }
        public static bool AutoCheck
        {
            get => _info.AutoCheck;
            set
            {
                if (_info.AutoCheck != value)
                {
                    _info.AutoCheck = value;
                    // 关闭自动检查更新后恢复默认设置
                    if (!value)
                    {
                        _info.AutoUpdate = false;
                        _info.CheckInterval = 7;
                    }
                    SaveConfig();
                }
            }
        }

        public static bool AutoUpdate
        {
            get => _info.AutoUpdate;
            set
            {
                if (_info.AutoUpdate != value)
                {
                    _info.AutoUpdate = value;
                    SaveConfig();
                }
            }
        }

        public static int CheckInterval
        {
            get => _info.CheckInterval;
            set
            {
                if (_info.CheckInterval != value && value >= 0 && value <= 90)
                {
                    _info.CheckInterval = value;
                    SaveConfig();
                }
            }
        }

        /// <summary>
        /// 初始化 bangumi-data 数据，
        /// 读取文件，将数据加载到内存
        /// </summary>
        /// <param name="dataFolderPath">文件夹路径</param>
        /// <param name="useBiliApp">是否将链接转换为使用 哔哩哔哩动画 启动协议</param>
        public static void Init(string dataFolderPath, bool useBiliApp = false, Action<string> autoCheckCallback = null)
        {
            _folderPath = dataFolderPath ?? throw new ArgumentNullException(nameof(dataFolderPath));
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
            UseBiliApp = useBiliApp;
            Task.Run(async () =>
            {
                try
                {
                    _dataSet = BangumiDataSet.FromJson(await FileHelper.ReadTextAsync(AppFile.Data_json.GetFilePath(_folderPath)));
                    // 从老版本升级
                    if (File.Exists(AppFile.Version.GetFilePath(_folderPath)))
                    {
                        _info = new VersionInfo
                        {
                            Version = await FileHelper.ReadTextAsync(AppFile.Version.GetFilePath(_folderPath))
                        };
                        await SaveConfig();
                        FileHelper.DeleteFile(AppFile.Version.GetFilePath(_folderPath));
                        return;
                    }
                    _info = VersionInfo.FromJson(await FileHelper.ReadTextAsync(AppFile.Config_json.GetFilePath(_folderPath)));
                }
                catch
                {
                    FileHelper.DeleteFile(AppFile.Data_json.GetFilePath(_folderPath));
                    FileHelper.DeleteFile(AppFile.Config_json.GetFilePath(_folderPath));
                }
                _info ??= new VersionInfo();
                // 未设置站点时，设置默认值
                if (_info.SitesEnabledOrder == null)
                {
                    _info.SitesEnabledOrder = _dataSet.SiteMeta.Where(it => it.Value.Type == "onair").Select(it => it.Key).ToArray();
                    await SaveConfig();
                }
            }).Wait();
            Task.Run(async () =>
            {
                // 自动检查更新
                if (_info.AutoCheck && DateTimeOffset.UtcNow.Date >= _info.LastUpdate.Date.AddDays(_info.CheckInterval))
                {
                    if (_info.AutoUpdate)
                    {
                        bool hasNew = false;
                        if (await DownloadLatestBangumiData(() => hasNew = true) && hasNew)
                        {
                            autoCheckCallback?.Invoke("bangumi-data 数据已更新！");
                        }
                    }
                    else
                    {
                        if (Version != await GetLatestVersion())
                        {
                            autoCheckCallback?.Invoke("发现新版本 bangumi-data，请前往设置手动更新！");
                        }
                        else
                        {
                            _info.LastUpdate = DateTimeOffset.UtcNow;
                            await SaveConfig();
                        }
                    }
                }
            });
        }


        #region 公共方法
        /// <summary>
        /// 根据网站放送开始时间推测更新时间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DateTimeOffset? GetAirTimeByBangumiId(string id)
        {
            var siteList = GetOrderedSitesByBangumiId(id);
            foreach (var site in siteList)
            {
                if (!site.Begin.HasValue) continue;
                return site.Begin.Value;
            }

            return null;
        }

        /// <summary>
        /// 根据Bangumi的ID返回启用的排好序的放送网站
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<Site>> GetAirSitesByBangumiIdAsync(string id)
        {
            var siteList = GetOrderedSitesByBangumiId(id).ToList();
            foreach (var site in siteList)
            {
                if (!string.IsNullOrEmpty(site.Id))
                {
                    site.Url = _dataSet.SiteMeta[site.SiteName].UrlTemplate.Replace("{{id}}", site.Id);
                }
            }

            // 启用设置，将mediaid转换为seasonid
            if (UseBiliApp)
            {
                var biliSite = siteList.FirstOrDefault(s => s.SiteName == "bilibili");
                if (biliSite != null)
                {
                    if (!_seasonIdMap.TryGetValue(biliSite.Id, out var seasonId))
                    {
                        var url = $"https://bangumi.bilibili.com/view/web_api/media?media_id={biliSite.Id}";
                        try
                        {
                            var result = await url.GetStringAsync();
                            JObject jObject = JObject.Parse(result);
                            seasonId = jObject.SelectToken("result.param.season_id").ToString();
                            _seasonIdMap.Add(biliSite.Id, seasonId);
                            _ = FileHelper.WriteTextAsync(AppFile.Map_json.GetFilePath(_folderPath),
                                                     JsonConvert.SerializeObject(_seasonIdMap));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("获取seasonId失败");
                            Debug.WriteLine(e.Message);
                            return siteList;
                        }
                    }

                    biliSite.Url = "bilibili://bangumi/season/" + seasonId;
                }
            }

            siteList.ForEach(it => it.SiteName = _dataSet.SiteMeta[it.SiteName].Title);
            return siteList;
        }

        /// <summary>
        /// 获取未启用的站点
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, SiteMeta> GetNotEnabledSites()
        {
            var sites = new Dictionary<string, SiteMeta>();
            foreach (var item in _dataSet.SiteMeta)
            {
                if (!_info.SitesEnabledOrder.Contains(item.Key) && item.Key != "bangumi")
                {
                    sites.Add(item.Key, item.Value);
                }
            }
            return sites;
        }

        /// <summary>
        /// 获取已启用站点以及顺序
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, SiteMeta> GetEnabledSites()
        {
            var sites = new Dictionary<string, SiteMeta>();
            foreach (var site in _info.SitesEnabledOrder)
            {
                sites.Add(site, _dataSet.SiteMeta[site]);
            }
            return sites;
        }

        /// <summary>
        /// 设置启用的站点以及顺序
        /// </summary>
        /// <param name="siteKeys"></param>
        public static async Task SetSitesEnabledOrder(string[] siteKeys)
        {
            _info.SitesEnabledOrder = siteKeys;
            await SaveConfig();
        }
        #endregion

        #region 版本更新
        /// <summary>
        /// 获取最新版本号，并暂存
        /// </summary>
        /// <returns>返回最新版本号</returns>
        public static async Task<string> GetLatestVersion()
        {
            try
            {
                var result = await BangumiDataUrl.WithHeader("User-Agent", "Bangumi UWP").GetStringAsync();
                JArray jArray = JArray.Parse(result);
                // 返回第一个 tag 版本号
                LatestVersion = jArray[0].SelectToken("name").ToString();
                return LatestVersion;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取最新版本并下载数据
        /// </summary>
        /// <param name="startDownloadCallback">获取到最新版本后回调</param>
        /// <returns></returns>
        public static async Task<bool> DownloadLatestBangumiData(Action startDownloadCallback)
        {
            // 获取最新版本
            if (string.IsNullOrEmpty(await GetLatestVersion()))
            {
                return false;
            }
            // 已是最新版本
            if (_info.Version == LatestVersion)
            {
                _info.LastUpdate = DateTimeOffset.UtcNow;
                await SaveConfig();
                return true;
            }
            startDownloadCallback?.Invoke();
            try
            {
                // 下载并保存数据
                var data = await BangumiDataCDNUrl.GetStringAsync();
                _dataSet = JsonConvert.DeserializeObject<BangumiDataSet>(data);
                await FileHelper.WriteTextAsync(AppFile.Data_json.GetFilePath(_folderPath), data);
                _info.Version = LatestVersion;
                _info.LastUpdate = DateTimeOffset.UtcNow;
                await SaveConfig();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 根据Bangumi的ID返回所有放送网站
        /// </summary>
        private static IEnumerable<Site> GetOrderedSitesByBangumiId(string id)
        {
            var bangumiItem = GetItemByBangumiId(id);
            var sites = bangumiItem?.Sites;
            if (sites == null)
            {
                yield break;
            }
            foreach (var item in _info.SitesEnabledOrder)
            {
                // 未标明的资源站使用番剧标题作为ID
                if (_dataSet.SiteMeta[item].Type == "resource" && !sites.Any(it => it.SiteName == item))
                {
                    yield return new Site
                    {
                        SiteName = item,
                        Id = bangumiItem.TitleTranslate?.ZhHans.FirstOrDefault() ??
                             bangumiItem.TitleTranslate?.ZhHant.FirstOrDefault() ??
                             bangumiItem.TitleTranslate?.En.FirstOrDefault() ??
                             bangumiItem.Title
                    };
                }
                var site = sites.FirstOrDefault(it => it.SiteName == item);
                if (site == null)
                {
                    continue;
                }
                yield return site.Clone();
            }
        }

        private static Item GetItemByBangumiId(string id)
        {
            return _dataSet?.Items.FirstOrDefault(e => e.Sites.Any(s => s.SiteName == "bangumi" && s.Id == id));
        }

        private static Task SaveConfig()
        {
            return FileHelper.WriteTextAsync(AppFile.Config_json.GetFilePath(_folderPath), _info.ToJson());
        }
        #endregion


        #region AppFile
        /// <summary>
        /// 使用的文件
        /// </summary>
        private enum AppFile
        {
            Data_json,
            Map_json,
            Config_json,
            Version,
        }

        /// <summary>
        /// 文件名转换为小写，
        /// 与文件夹组合为路径，
        /// 将 '_' 替换为 '.'
        /// </summary>
        /// <param name="file"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static string GetFilePath(this AppFile file, string folder)
        {
            return Path.Combine(folder, file.ToString().ToLowerInvariant().Replace('_', '.'));
        }

        #endregion


    }
}
