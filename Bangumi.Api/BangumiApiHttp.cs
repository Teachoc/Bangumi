using Bangumi.Api.Models;
using Bangumi.Api.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bangumi.Api
{
    /// <summary>
    /// BangumiApi 的 Http 部分
    /// </summary>
    public static partial class BangumiApi
    {
        /// <summary>
        /// Http 请求封装
        /// </summary>
        private static BangumiHttpWrapper _wrapper;

        /// <summary>
        /// 检查网络状态委托
        /// </summary>
        private static Func<bool> _checkNetworkAction;

        /// <summary>
        /// 表示是否处于离线模式
        /// </summary>
        private static bool _isOffline;


        /// <summary>
        /// 重新检查网络状态
        /// </summary>
        public static void RecheckNetworkStatus()
        {
            _isOffline = _checkNetworkAction();
        }

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

    }
}
