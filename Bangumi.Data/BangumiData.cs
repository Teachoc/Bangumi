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
        private static BangumiDataSet dataSet;
        private static Dictionary<string, string> seasonIdMap;
        private static string latestVersion;
        private static string folderPath;

        public static string Version { get; private set; }
        public static bool UseBiliApp { get; set; }

        /// <summary>
        /// 读取文件，将数据加载到内存
        /// </summary>
        /// <param name="datafolderpath">文件夹路径</param>
        public static async Task Init(string datafolderpath, bool useBiliApp = false)
        {
            folderPath = datafolderpath;
            UseBiliApp = useBiliApp;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (dataSet == null &&
                File.Exists(AppFile.Data_json.GetFilePath(folderPath)) &&
                File.Exists(AppFile.Version.GetFilePath(folderPath)))
            {
                dataSet = JsonConvert.DeserializeObject<BangumiDataSet>(await FileHelper.ReadTextAsync(AppFile.Data_json.GetFilePath(folderPath)));
                Version = await FileHelper.ReadTextAsync(AppFile.Version.GetFilePath(folderPath));
            }
            if (seasonIdMap == null)
            {
                if (File.Exists(AppFile.Map_json.GetFilePath(folderPath)))
                {
                    seasonIdMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(await FileHelper.ReadTextAsync(AppFile.Map_json.GetFilePath(folderPath)));
                }
                else
                {
                    seasonIdMap = new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// 解析网页获取最新版本号，并暂存
        /// </summary>
        /// <returns>返回最新版本号</returns>
        public static async Task<string> GetLatestVersion()
        {
            try
            {
                var result = await BangumiDataUrl.WithHeader("User-Agent", "Bangumi UWP").GetStringAsync();
                JArray jArray = JArray.Parse(result);
                // 返回第一个 tag 版本号
                latestVersion = jArray[0].SelectToken("name").ToString();
                return latestVersion;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return "";
            }
        }

        /// <summary>
        /// 下载最新的数据，并更新原有数据，不比较版本号
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> DownloadLatestBangumiData()
        {
            if (string.IsNullOrEmpty(latestVersion))
                await GetLatestVersion();
            if (!string.IsNullOrEmpty(latestVersion))
            {
                try
                {
                    var data = await BangumiDataCDNUrl.GetStringAsync();
                    dataSet = JsonConvert.DeserializeObject<BangumiDataSet>(data);
                    await FileHelper.WriteTextAsync(AppFile.Data_json.GetFilePath(folderPath), data);
                    Version = latestVersion;
                    await FileHelper.WriteTextAsync(AppFile.Version.GetFilePath(folderPath), Version);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据Bangumi的ID返回所有放送网站
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<Site>> GetAirSitesByBangumiIdAsync(string id)
        {
            if (dataSet == null)
            {
                return new List<Site>();
            }

            var siteList = dataSet.Items.Where(e => e.Sites.Where(s => s.SiteName == "bangumi" && s.Id == id).Count() != 0)
                                          .FirstOrDefault()?.Sites
                                          .Where(s => dataSet.SiteMeta[s.SiteName].Type == "onair").ToList();
            if (siteList != null)
            {
                foreach (var site in siteList)
                {
                    site.Url = string.IsNullOrEmpty(site.Id) ?
                               site.Url :
                               dataSet.SiteMeta[site.SiteName].UrlTemplate.Replace("{{id}}", site.Id);
                }
                // 启用设置，将mediaid转换为seasonid
                if (UseBiliApp)
                {
                    var biliSite = siteList.Where(s => s.SiteName == "bilibili").FirstOrDefault();
                    if (biliSite != null)
                    {
                        string seasonId;
                        if (!seasonIdMap.TryGetValue(biliSite.Id, out seasonId))
                        {
                            var url = string.Format("https://bangumi.bilibili.com/view/web_api/media?media_id={0}", biliSite.Id);
                            try
                            {
                                var result = await url.GetStringAsync();
                                JObject jObject = JObject.Parse(result);
                                seasonId = jObject.SelectToken("result.param.season_id").ToString();
                                seasonIdMap.Add(biliSite.Id, seasonId);
                                _ = FileHelper.WriteTextAsync(AppFile.Map_json.GetFilePath(folderPath), JsonConvert.SerializeObject(seasonIdMap));
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
                return siteList;
            }
            else
            {
                return new List<Site>();
            }
        }


        #region AppFile
        /// <summary>
        /// 使用的文件
        /// </summary>
        private enum AppFile
        {
            Data_json,
            Version,
            Map_json,
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
            return Path.Combine(folder, file.ToString().ToLower().Replace('_', '.'));
        }

        #endregion


    }
}
