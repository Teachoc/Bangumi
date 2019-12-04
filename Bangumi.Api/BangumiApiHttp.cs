using Bangumi.Api.Models;
using Bangumi.Api.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public static (Collection2, Task<Collection2>) GetSubjectCollectionAsync(
            SubjectTypeEnum subjectType, RequestType requestType = RequestType.All)
        {
            try
            {
                Collection2 cache = null;
                Task<Collection2> response = null;
                if (requestType != RequestType.TaskOnly)
                {
                    BangumiCache.Collections.TryGetValue(subjectType.GetValue(), out cache);
                }
                if (requestType != RequestType.CacheOnly)
                {
                    response = _wrapper.GetSubjectCollectionAsync(MyToken.UserId, subjectType)
                        .ContinueWith(t => UpdateCache(BangumiCache.Collections, subjectType.GetValue(), t.Result),
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                return (cache, response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectCollectionAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
            }
        }

        /// <summary>
        /// 获取指定条目收藏信息。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static (SubjectStatus2, Task<SubjectStatus2>) GetSubjectStatusAsync(
            string subjectId, RequestType requestType = RequestType.All)
        {
            try
            {
                SubjectStatus2 subjectStatusCache = null;
                Task<SubjectStatus2> response = null;
                if (requestType != RequestType.TaskOnly)
                {
                    BangumiCache.SubjectStatus.TryGetValue(subjectId, out subjectStatusCache);
                }
                if (requestType != RequestType.CacheOnly)
                {
                    response = _wrapper.GetSubjectStatusAsync(MyToken.Token, subjectId)
                        .ContinueWith(t => UpdateCache(BangumiCache.SubjectStatus, subjectId, t.Result),
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                return (subjectStatusCache, response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetCollectionStatusAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
            }
        }

        /// <summary>
        /// 获取用户指定条目收视进度。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static (Progress, Task<Progress>) GetProgressesAsync(
            string subjectId, RequestType requestType = RequestType.All)
        {
            try
            {
                Progress progressCache = null;
                Task<Progress> response = null;
                if (requestType != RequestType.TaskOnly)
                {
                    BangumiCache.Progresses.TryGetValue(subjectId, out progressCache);
                }
                if (requestType != RequestType.CacheOnly)
                {
                    response = _wrapper.GetProgressesAsync(MyToken.UserId, MyToken.Token, subjectId)
                        .ContinueWith(t => UpdateCache(BangumiCache.Progresses, subjectId, t.Result),
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                return (progressCache, response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetProgressesAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
            }
        }

        /// <summary>
        /// 获取用户收视列表。
        /// </summary>
        /// <returns></returns>
        public static (List<Watching>, Task<List<Watching>>) GetWatchingListAsync(
            RequestType requestType = RequestType.All)
        {
            try
            {
                List<Watching> watchingCache = null;
                Task<List<Watching>> response = null;
                if (requestType != RequestType.TaskOnly)
                {
                    watchingCache = BangumiCache.Watchings.ToList();
                }
                if (requestType != RequestType.CacheOnly)
                {
                    response = _wrapper.GetWatchingListAsync(MyToken.UserId)
                        .ContinueWith(t => UpdateCache(BangumiCache.Watchings, t.Result),
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                return (watchingCache, response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetWatchingListAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
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
        public static async Task<bool> UpdateCollectionStatusAsync(
            string subjectId, CollectionStatusEnum collectionStatusEnum,
            string comment = "", string rating = "", string privace = "0")
        {
            try
            {
                if (_isOffline)
                {
                    throw new Exception("当前处于离线模式");
                }
                var response = _wrapper.UpdateCollectionStatusAsync(MyToken.Token,
                    subjectId, collectionStatusEnum, comment, rating, privace)
                    .ContinueWith(t => UpdateSubjectStatusCache(subjectId, t.Result),
                    TaskContinuationOptions.OnlyOnRanToCompletion);
                return (await response)?.Status.Type == collectionStatusEnum.GetValue();
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateCollectionStatusAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
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
                return await _wrapper.UpdateProgressAsync(MyToken.Token, epId, status)
                    .ContinueWith(t =>
                    {
                        if (t.Result)
                        {
                            UpdateProgressCache(int.Parse(epId), status);
                        }
                        return t.Result;
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
            }
        }

        /// <summary>
        /// 批量更新收视进度。
        /// 仅支持更新状态为已看。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status">必须为 <see cref="EpStatusEnum.watched"/> 值</param>
        /// <param name="epsId">章节id，逗号分隔</param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressBatchAsync(int ep, EpStatusEnum status, string epsId)
        {
            if (status != EpStatusEnum.watched)
            {
                throw new NotSupportedException("Batch update progress currently only support watched.");
            }

            try
            {
                if (_isOffline)
                {
                    throw new Exception("当前处于离线模式");
                }
                var result = await _wrapper.UpdateProgressBatchAsync(MyToken.Token, ep, status, epsId)
                    .ContinueWith(t =>
                    {
                        if (t.Result)
                        {
                            foreach (var ep in epsId.Split(','))
                            {
                                UpdateProgressCache(int.Parse(ep), status);
                            }
                        }
                        return t.Result;
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion);

                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressBatchAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
            }
        }

        /// <summary>
        /// 获取指定条目所有章节。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static (Subject, Task<Subject>) GetSubjectEpsAsync(
            string subjectId, RequestType requestType = RequestType.All)
        {
            try
            {
                // 若缓存中已有该条目，则只获取 Ep 信息，
                // 否则获取完整信息
                if (BangumiCache.Subjects.ContainsKey(subjectId))
                {
                    Subject subjectCache = null;
                    Task<Subject> response = null;
                    if (requestType != RequestType.TaskOnly)
                    {
                        BangumiCache.Subjects.TryGetValue(subjectId, out subjectCache);
                    }
                    if (requestType != RequestType.CacheOnly)
                    {
                        response = _wrapper.GetSubjectEpsAsync(subjectId)
                            .ContinueWith(t =>
                            {
                                UpdateCache(BangumiCache.Subjects[subjectId].Eps, t.Result.Eps);
                                return t.Result;
                            },
                            TaskContinuationOptions.OnlyOnRanToCompletion);
                    }
                    return (subjectCache, response);
                }
                else
                {
                    return GetSubjectAsync(subjectId, requestType);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectEpsAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
            }
        }

        /// <summary>
        /// 获取指定条目详情。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static (Subject, Task<Subject>) GetSubjectAsync(
            string subjectId, RequestType requestType = RequestType.All)
        {
            try
            {
                Subject subjectCache = null;
                Task<Subject> response = null;
                if (requestType != RequestType.TaskOnly)
                {
                    BangumiCache.Subjects.TryGetValue(subjectId, out subjectCache);
                }
                if (requestType != RequestType.CacheOnly)
                {
                    response = _wrapper.GetSubjectAsync(subjectId)
                        .ContinueWith(t => UpdateCache(BangumiCache.Subjects, subjectId, t.Result),
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                return (subjectCache, response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
            }
        }

        /// <summary>
        /// 获取时间表。
        /// </summary>
        /// <returns></returns>
        public static (List<BangumiTimeLine>, Task<List<BangumiTimeLine>>) GetBangumiTimelineAsync(
            RequestType requestType = RequestType.All)
        {
            try
            {
                List<BangumiTimeLine> timelineCache = null;
                Task<List<BangumiTimeLine>> response = null;
                if (requestType != RequestType.TaskOnly)
                {
                    timelineCache = BangumiCache.Timeline.ToList();
                }
                if (requestType != RequestType.CacheOnly)
                {
                    response = _wrapper.GetBangumiTimelineAsync()
                       .ContinueWith(t => UpdateCache(BangumiCache.Timeline, t.Result),
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                return (timelineCache, response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetBangumiCalendarAsync Error.");
                Debug.WriteLine(e);
                RecheckNetworkStatus();
                throw;
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
                throw;
            }
        }


        /// <summary>
        /// 请求返回种类
        /// </summary>
        public enum RequestType
        {
            All = 0,
            CacheOnly = 1,
            TaskOnly = 2
        }


    }
}
