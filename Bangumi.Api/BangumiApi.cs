using Bangumi.Api.Models;
using Bangumi.Api.Services;
using Bangumi.Api.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Bangumi.Api
{
    /// <summary>
    /// 提供 Api 访问以及缓存管理
    /// </summary>
    public static class BangumiApi
    {
        /// <summary>
        /// Http 请求封装
        /// </summary>
        private static BangumiHttpWrapper _wrapper;

        /// <summary>
        /// 本地文件夹路径，永久保存
        /// </summary>
        private static string _localFolderPath;

        /// <summary>
        /// 缓存文件夹路径
        /// </summary>
        private static string _cacheFolderPath;

        /// <summary>
        /// 用来表示 Token 过期或不可用
        /// </summary>
        private static bool _isLogin = true;

        /// <summary>
        /// 记录缓存是否更新过
        /// </summary>
        private static bool _isCacheUpdated = false;

        /// <summary>
        /// 定时器
        /// </summary>
        private static Timer _timer;

        /// <summary>
        /// 定时器触发间隔
        /// </summary>
        private const int _interval = 30000;

        /// <summary>
        /// 检查网络状态委托
        /// </summary>
        private static CheckNetworkDelegate _checkNetworkAction;

        /// <summary>
        /// 表示是否处于离线模式
        /// </summary>
        private static bool _isOffline;


        public delegate bool CheckNetworkDelegate();

        public static string OAuthBaseUrl => _wrapper.OAuthBaseUrl;
        public static string ClientId => _wrapper.ClientId;
        public static string RedirectUrl => _wrapper.RedirectUrl;

        /// <summary>
        /// 用户认证存在且可用
        /// </summary>
        public static bool IsLogin { get => MyToken != null && _isLogin; }

        /// <summary>
        /// 存储 Token
        /// </summary>
        public static AccessToken MyToken { get; private set; }

        /// <summary>
        /// 存储缓存数据
        /// </summary>
        public static BangumiCache BangumiCache { get; private set; }

        /// <summary>
        /// 初始化 Api
        /// </summary>
        /// <param name="localFolder">本地文件夹</param>
        /// <param name="cacheFolder">缓存文件夹</param>
        /// <param name="baseUrl"></param>
        /// <param name="oAuthBaseUrl"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="redirectUrl"></param>
        /// <param name="noImageUri">无图片时显示的图片路径</param>
        /// <param name="encryptionDelegate">加密方法</param>
        /// <param name="decryptionDelegate">解密方法</param>
        /// <param name="checkNetworkActivityDelegate">检查网络是否可用的方法</param>
        public static async void Init(
            string localFolder,
            string cacheFolder,
            string baseUrl,
            string oAuthBaseUrl,
            string clientId,
            string clientSecret,
            string redirectUrl,
            string noImageUri,
            FileHelper.EncryptionDelegate encryptionDelegate,
            FileHelper.DecryptionDelegate decryptionDelegate,
            CheckNetworkDelegate checkNetworkActivityDelegate)
        {

            FileHelper.EncryptionAsync = encryptionDelegate ?? throw new ArgumentNullException(nameof(encryptionDelegate));
            FileHelper.DecryptionAsync = decryptionDelegate ?? throw new ArgumentNullException(nameof(decryptionDelegate));
            _checkNetworkAction = checkNetworkActivityDelegate ?? throw new ArgumentNullException(nameof(checkNetworkActivityDelegate));
            _isOffline = _checkNetworkAction();
            if (_wrapper == null && BangumiCache == null && _timer == null)
            {
                _localFolderPath = localFolder ?? throw new ArgumentNullException(nameof(localFolder));
                _cacheFolderPath = cacheFolder ?? throw new ArgumentNullException(nameof(cacheFolder));
                _wrapper = new BangumiHttpWrapper
                {
                    BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl)),
                    OAuthBaseUrl = oAuthBaseUrl ?? throw new ArgumentNullException(nameof(oAuthBaseUrl)),
                    ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId)),
                    ClientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret)),
                    RedirectUrl = redirectUrl ?? throw new ArgumentNullException(nameof(redirectUrl)),
                    NoImageUri = noImageUri ?? throw new ArgumentNullException(nameof(noImageUri))
                };

                // 加载缓存
                BangumiCache = new BangumiCache();
                if (File.Exists(AppFile.BangumiCache.GetFilePath(_cacheFolderPath)))
                {
                    try
                    {
                        BangumiCache = JsonConvert.DeserializeObject<BangumiCache>(await FileHelper.ReadTextAsync(AppFile.BangumiCache.GetFilePath(_cacheFolderPath)));
                    }
                    catch (Exception)
                    {
                        FileHelper.DeleteFile(AppFile.BangumiCache.GetFilePath(_cacheFolderPath));
                    }
                }

                // 启动定时器，定时将缓存写入文件，30 秒
                _timer = new Timer(_interval);
                _timer.Elapsed += WriteCacheToFileTimer_Elapsed;
                _timer.AutoReset = true;
                _timer.Start();
            }
            else
            {
                throw new InvalidOperationException("BangumiApi can't init twice!");
            }
        }

        /// <summary>
        /// 重新检查网络状态
        /// </summary>
        public static void RecheckNetworkStatus()
        {
            _isOffline = _checkNetworkAction();
        }

        #region 缓存操作公开方法

        /// <summary>
        /// 将缓存写入文件
        /// </summary>
        /// <returns></returns>
        public static async Task WriteCacheToFileRightNow()
        {
            if (_isCacheUpdated)
            {
                _isCacheUpdated = false;
                await FileHelper.WriteTextAsync(AppFile.BangumiCache.GetFilePath(_cacheFolderPath),
                                                JsonConvert.SerializeObject(BangumiCache));
            }
        }

        /// <summary>
        /// 清楚缓存并删除缓存文件
        /// </summary>
        public static void DeleteCache()
        {
            _isCacheUpdated = false;
            BangumiCache = null;
            BangumiCache = new BangumiCache();
            FileHelper.DeleteFile(AppFile.BangumiCache.GetFilePath(_cacheFolderPath));
        }

        /// <summary>
        /// 获取缓存文件大小
        /// </summary>
        /// <returns></returns>
        public static long GetCacheFileLength()
        {
            return FileHelper.GetFileLength(AppFile.BangumiCache.GetFilePath(_cacheFolderPath));
        }

        #endregion


        #region Api 请求
        /// <summary>
        /// 获取指定类别收藏信息。
        /// </summary>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        public static async Task<Collection2> GetSubjectCollectionAsync(SubjectTypeEnum subjectType)
        {
            try
            {
                if (_isOffline)
                {
                    BangumiCache.Collections.TryGetValue(subjectType.GetValue(), out Collection2 cache);
                    return cache;
                }
                var result = await _wrapper.GetSubjectCollectionAsync(MyToken.UserId, subjectType);
                UpdateCache(BangumiCache.Collections, subjectType.GetValue(), result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectCollectionAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目收藏信息。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<SubjectStatus2> GetCollectionStatusAsync(string subjectId)
        {
            try
            {
                if (_isOffline)
                {
                    BangumiCache.SubjectStatus.TryGetValue(subjectId, out SubjectStatus2 subjectStatusCache);
                    return subjectStatusCache;
                }
                var result = await _wrapper.GetCollectionStatusAsync(MyToken.Token, subjectId);
                UpdateCache(BangumiCache.SubjectStatus, subjectId, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetCollectionStatusAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        /// <summary>
        /// 获取用户指定条目收视进度。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Progress> GetProgressesAsync(string subjectId)
        {
            try
            {
                if (_isOffline)
                {
                    BangumiCache.Progresses.TryGetValue(subjectId, out Progress progressCache);
                    return progressCache;
                }
                var result = await _wrapper.GetProgressesAsync(MyToken.UserId, MyToken.Token, subjectId);
                UpdateCache(BangumiCache.Progresses, subjectId, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetProgressesAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        /// <summary>
        /// 获取用户收视列表。
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Watching>> GetWatchingListAsync()
        {
            try
            {
                if (_isOffline)
                {
                    return BangumiCache.Watchings;
                }
                var result = await _wrapper.GetWatchingListAsync(MyToken.UserId);
                UpdateCache(ref BangumiCache._watchings, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetWatchingListAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        /// <summary>
        /// 更新指定条目收藏状态。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="collectionStatusEnum"></param>
        /// <param name="comment"></param>
        /// <param name="rating"></param>
        /// <param name="privace"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateCollectionStatusAsync(string subjectId,
                                                                   CollectionStatusEnum collectionStatusEnum,
                                                                   string comment = "",
                                                                   string rating = "",
                                                                   string privace = "0")
        {
            try
            {
                if (_isOffline)
                {
                    throw new Exception("当前处于离线模式");
                }
                var result = await _wrapper.UpdateCollectionStatusAsync(MyToken.Token, subjectId,
                                    collectionStatusEnum, comment, rating, privace);
                if (result)
                {
                    UpdateSubjectStatusCache(subjectId, collectionStatusEnum, comment, rating, privace);
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateCollectionStatusAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        /// <summary>
        /// 更新收视进度。
        /// </summary>
        /// <param name="epId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressAsync(string epId, EpStatusEnum status)
        {
            try
            {
                if (_isOffline)
                {
                    throw new Exception("当前处于离线模式");
                }
                var result = await _wrapper.UpdateProgressAsync(MyToken.Token, epId, status);
                if (result)
                {
                    UpdateProgressCache(int.Parse(epId), status);
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        /// <summary>
        /// 批量更新收视进度。
        /// 使用 HttpWebRequest 提交表单进行更新，更新收藏状态使用相同方法。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <param name="epsId"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressBatchAsync(int ep, EpStatusEnum status, string epsId)
        {
            try
            {
                if (_isOffline)
                {
                    throw new Exception("当前处于离线模式");
                }
                var result = await _wrapper.UpdateProgressBatchAsync(MyToken.Token, ep, status, epsId);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressBatchAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目所有章节。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Subject> GetSubjectEpsAsync(string subjectId)
        {
            try
            {
                if (_isOffline)
                {
                    BangumiCache.Subjects.TryGetValue(subjectId, out Subject subjectCache);
                    return subjectCache;
                }
                Subject result;
                // 若缓存中已有该条目，则只获取 Ep 信息，
                // 否则获取完整信息
                if (BangumiCache.Subjects.ContainsKey(subjectId))
                {
                    result = await _wrapper.GetSubjectEpsAsync(subjectId);
                    UpdateCache(ref BangumiCache.Subjects[subjectId]._eps, result._eps);
                }
                else
                {
                    result = await GetSubjectAsync(subjectId);
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectEpsAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目详情。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Subject> GetSubjectAsync(string subjectId)
        {
            try
            {
                if (_isOffline)
                {
                    BangumiCache.Subjects.TryGetValue(subjectId, out Subject subjectCache);
                    return subjectCache;
                }
                var result = await _wrapper.GetSubjectAsync(subjectId);
                UpdateCache(BangumiCache.Subjects, subjectId, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        /// <summary>
        /// 获取时间表。
        /// </summary>
        /// <returns></returns>
        public static async Task<List<BangumiTimeLine>> GetBangumiCalendarAsync()
        {
            try
            {
                if (_isOffline)
                {
                    return BangumiCache.TimeLine;
                }
                var result = await _wrapper.GetBangumiCalendarAsync();
                UpdateCache(ref BangumiCache._timeLine, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetBangumiCalendarAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }
        /// <summary>
        /// 获取搜索结果。
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="type"></param>
        /// <param name="start"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static async Task<SearchResult> GetSearchResultAsync(string keyWord, string type, int start, int n)
        {
            try
            {
                if (_isOffline)
                {
                    throw new Exception("当前处于离线模式");
                }
                var result = await _wrapper.GetSearchResultAsync(keyWord, type, start, n);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSearchResultAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw e;
            }
        }

        #endregion


        #region 内部更新缓存方法

        /// <summary>
        /// 定时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void WriteCacheToFileTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await WriteCacheToFileRightNow();
        }

        /// <summary>
        /// 更新缓存记录的条目状态
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="collectionStatus"></param>
        /// <param name="comment"></param>
        /// <param name="rating"></param>
        /// <param name="privace"></param>
        private static void UpdateSubjectStatusCache(string subjectId, CollectionStatusEnum collectionStatus,
                                                     string comment, string rating, string privace)
        {
            BangumiCache.SubjectStatus.TryGetValue(subjectId, out var status);
            if (status != null)
            {
                _isCacheUpdated = false;
                if (collectionStatus != CollectionStatusEnum.No)
                {
                    status.Status = new SubjectStatus()
                    {
                        Id = (int)collectionStatus,
                        Type = collectionStatus.GetValue(),
                    };
                    status.Comment = comment;
                    status.Rating = string.IsNullOrEmpty(rating) ? 0 : int.Parse(rating);
                    status.Private = privace;
                }
                else
                {
                    BangumiCache.SubjectStatus.Remove(subjectId);
                }
                _isCacheUpdated = true;
            }
        }

        /// <summary>
        /// 更新缓存记录的章节状态
        /// </summary>
        /// <param name="epId"></param>
        /// <param name="status"></param>
        private static void UpdateProgressCache(int epId, EpStatusEnum status)
        {
            // 找到该章节所属的条目
            var sub = BangumiCache.Subjects.Values.Where(s => s.Eps.Where(p => p.Id == epId).FirstOrDefault() != null).FirstOrDefault();
            if (sub != null)
            {
                _isCacheUpdated = false;
                // 找到已有进度，否则新建
                var pro = BangumiCache.Progresses.Values.Where(p => p.SubjectId == sub.Id).FirstOrDefault();
                if (pro != null)
                {
                    var ep = pro.Eps.Where(e => e.Id == epId).FirstOrDefault();
                    if (status != EpStatusEnum.remove)
                    {
                        if (ep != null)
                        {
                            ep.Status.Id = (int)status;
                            ep.Status.CnName = status.GetCnName();
                            ep.Status.CssName = status.GetCssName();
                            ep.Status.UrlName = status.GetUrlName();
                        }
                        else
                        {
                            pro.Eps.Add(new EpStatus2()
                            {
                                Id = epId,
                                Status = new EpStatus()
                                {
                                    Id = (int)status,
                                    CnName = status.GetCnName(),
                                    CssName = status.GetCssName(),
                                    UrlName = status.GetUrlName(),
                                }
                            });
                        }
                    }
                    else
                    {
                        if (ep != null)
                        {
                            pro.Eps.Remove(ep);
                        }
                    }
                }
                else if (status != EpStatusEnum.remove)
                {

                    BangumiCache.Progresses.Add(sub.Id.ToString(), new Progress()
                    {
                        SubjectId = sub.Id,
                        Eps = new List<EpStatus2>()
                        {
                            new EpStatus2()
                            {
                                Id = epId,
                                Status = new EpStatus()
                                {
                                    Id = (int)status,
                                    CnName = status.GetCnName(),
                                    CssName = status.GetCssName(),
                                    UrlName = status.GetUrlName(),
                                }
                            }
                        }
                    });
                }
                // 找到收视列表中的条目，修改 LastTouch
                var watch = BangumiCache.Watchings.Where(w => w.SubjectId == sub.Id).FirstOrDefault();
                if (watch != null)
                {
                    watch.LastTouch = DateTime.Now.ConvertDateTimeToJsTick();
                }
                _isCacheUpdated = true;
            }
        }

        /// <summary>
        /// 更新缓存，字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void UpdateCache<T>(Dictionary<string, T> dic, string key, T value)
        {
            if (dic == null) throw new ArgumentNullException(nameof(dic));
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (dic.ContainsKey(key))
            {
                if (!dic[key].EqualsExT(value))
                {
                    _isCacheUpdated = false;
                    dic[key] = value;
                    _isCacheUpdated = true;
                }
            }
            else
            {
                _isCacheUpdated = false;
                dic.Add(key, value);
                _isCacheUpdated = true;
            }
        }

        /// <summary>
        /// 更新缓存，列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        private static void UpdateCache<T>(ref List<T> source, List<T> dest)
        {
            if (!source.SequenceEqualExT(dest))
            {
                _isCacheUpdated = false;
                source = dest;
                _isCacheUpdated = true;
            }
        }

        #endregion


        #region OAuth 相关方法
        #region public

        /// <summary>
        /// 使用 code 换取 Access Token。
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task GetTokenAsync(string code)
        {
            AccessToken token;
            // 重试最多三次
            for (int i = 0; i < 3; i++)
            {
                Debug.WriteLine($"第{i + 1}次尝试获取Token。");
                token = await _wrapper.GetTokenAsync(code);
                if (token != null)
                {
                    await WriteTokenAsync(token);
                    break;
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        /// <summary>
        /// 检查用户授权文件。
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CheckMyToken()
        {
            if (MyToken == null)
            {
                MyToken = JsonConvert.DeserializeObject<AccessToken>(await FileHelper.ReadAndDecryptFileAsync(AppFile.Token_Data.GetFilePath(_localFolderPath)));
                if (MyToken == null)
                {
                    //DeleteTokens();
                    return false;
                }
            }
            // 检查是否在有效期内，接近过期或过期则刷新token
            _isLogin = true;
            _ = CheckToken();
            return true;
        }

        /// <summary>
        /// 删除用户相关文件。
        /// </summary>
        /// <returns></returns>
        public static void DeleteToken()
        {
            // 删除用户认证文件
            MyToken = null;
            FileHelper.DeleteFile(AppFile.Token_Data.GetFilePath(_localFolderPath));
            // 清空用户缓存
            DeleteCache();
        }
        #endregion


        #region private

        /// <summary>
        /// 查询授权信息，并在满足条件时刷新Token。
        /// </summary>
        private static async Task CheckToken()
        {
            try
            {
                AccessToken token;
                Debug.WriteLine("尝试刷新Token。");
                token = await _wrapper.CheckTokenAsync(MyToken);
                if (token != null)
                {
                    // 将信息写入本地文件
                    if (!token.EqualsExT(MyToken))
                        await WriteTokenAsync(token);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("401"))
                {
                    _isLogin = false;
                }
                Debug.WriteLine(e.Message);
                RecheckNetworkStatus();
            }
        }

        /// <summary>
        /// 将 Token 写入内存及文件。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task WriteTokenAsync(AccessToken token)
        {
            // 存入内存
            MyToken = token;
            _isLogin = true;
            // 将信息写入本地文件
            await FileHelper.EncryptAndWriteFileAsync(AppFile.Token_Data.GetFilePath(_localFolderPath),
                                                      JsonConvert.SerializeObject(token));
        }
        #endregion
        #endregion


        #region AppFile

        /// <summary>
        /// 使用的文件
        /// </summary>
        private enum AppFile
        {
            Token_Data,
            BangumiCache,
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
