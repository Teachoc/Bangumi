using Bangumi.Api.Common;
using Bangumi.Api.Exceptions;
using Bangumi.Api.Extensions;
using Bangumi.Api.Models;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Bangumi.Api.Services
{
    public class BgmApi : IBgmApi
    {
        private const string HOST = "https://api.bgm.tv";
        private static IBgmCache _bgmCache;
        private static IBgmOAuth _bgmOAuth;

        public BgmApi(IBgmCache bgmCache, IBgmOAuth bgmOAuth)
        {
            _bgmCache = bgmCache ?? throw new ArgumentNullException(nameof(bgmCache));
            _bgmOAuth = bgmOAuth ?? throw new ArgumentNullException(nameof(bgmOAuth));

            FlurlHttp.ConfigureClient(HOST, client =>
            {
                client.Settings.BeforeCall = call =>
                {
                    if (_bgmOAuth.IsLogin)
                    {
                        call.Request.Headers.Add("Authorization", $"Bearer {_bgmOAuth.MyToken.Token}");
                    }
                    call.Request.Headers.Add("Cookie", $"chii_searchDateLine={DateTime.Now.ToString()}");
                };
                client.Settings.OnErrorAsync = async call =>
                {
                    if (call.HttpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // 检查Token
                        if (!await _bgmOAuth.CheckToken())
                        {
                            throw new BgmUnauthorizedException();
                        }
                    }
                    if (call.Exception is FlurlHttpTimeoutException)
                    {
                        throw new BgmTimeoutException();
                    }
                };
            });
        }

        /// <summary>
        /// 获取用户收视列表。
        /// </summary>
        /// <returns></returns>
        public async Task<List<Watching>> Watching()
        {
            return await $"{HOST}/user/{_bgmOAuth.MyToken.UserId}/collection"
                .SetQueryParams(new
                {
                    cat = "watching",
                    responseGroup = "medium"
                })
                .GetAsync()
                .ReceiveJson<List<Watching>>()
                .ContinueWith(t =>
                {
                    List<Watching> watchings = t.Result ?? new List<Watching>();
                    foreach (var watching in watchings)
                    {
                        watching.Subject.Name = System.Net.WebUtility.HtmlDecode(watching.Subject.Name);
                        watching.Subject.NameCn = watching.Subject.NameCn.IsNullOrEmpty() ?
                            watching.Subject.Name : System.Net.WebUtility.HtmlDecode(watching.Subject.NameCn);
                        watching.Subject.Images?.ConvertImageHttpToHttps();
                    }
                    return _bgmCache.UpdateWatching(watchings);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 获取指定类别收藏信息。
        /// </summary>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        public async Task<Collection2> Collections(SubjectTypeEnum subjectType)
        {
            return await $"{HOST}/user/{_bgmOAuth.MyToken.UserId}/collections/{subjectType.GetValue()}"
                .SetQueryParams(new
                {
                    app_id = BgmOAuth.ClientId,
                    max_results = 25
                })
                .GetAsync()
                .ReceiveJson<List<Collection2>>()
                .ContinueWith(t =>
                {
                    Collection2 collection = t.Result?.FirstOrDefault() ?? new Collection2 { Collects = new List<Collection>() };
                    foreach (var type in collection.Collects)
                    {
                        foreach (var item in type.Items)
                        {
                            item.Subject.Name = System.Net.WebUtility.HtmlDecode(item.Subject.Name);
                            item.Subject.NameCn = item.Subject.NameCn.IsNullOrEmpty() ?
                                item.Subject.Name : System.Net.WebUtility.HtmlDecode(item.Subject.NameCn);
                            item.Subject.Images?.ConvertImageHttpToHttps();
                        }
                    }
                    return _bgmCache.UpdateCollections(subjectType, collection);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 获取指定条目详情。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public async Task<Subject> Subject(string subjectId)
        {
            return await $"{HOST}/subject/{subjectId}?responseGroup=large"
                .GetAsync()
                .ReceiveJson<Subject>()
                .ContinueWith(t =>
                {
                    Subject subject = t.Result;
                    if (subject == null) return null;
                    subject.Name = System.Net.WebUtility.HtmlDecode(subject.Name);
                    subject.NameCn = System.Net.WebUtility.HtmlDecode(subject.NameCn);
                    subject.Images?.ConvertImageHttpToHttps();
                    // 将章节按类别排序
                    subject._eps = subject.Eps.OrderBy(c => c.Type).ThenBy(c => c.Sort).ToList();
                    foreach (var ep in subject.Eps)
                    {
                        ep.Name = System.Net.WebUtility.HtmlDecode(ep.Name);
                        ep.NameCn = ep.NameCn.IsNullOrEmpty() ? ep.Name : System.Net.WebUtility.HtmlDecode(ep.NameCn);
                    }
                    if (subject.Blogs != null)
                    {
                        foreach (var blog in subject.Blogs)
                        {
                            blog.Title = System.Net.WebUtility.HtmlDecode(blog.Title);
                            blog.Summary = System.Net.WebUtility.HtmlDecode(blog.Summary);
                        }
                    }
                    if (subject.Topics != null)
                    {
                        foreach (var topic in subject.Topics)
                        {
                            topic.Title = System.Net.WebUtility.HtmlDecode(topic.Title);
                        }
                    }
                    return _bgmCache.UpdateSubject(subjectId, subject);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 获取指定条目所有章节。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public async Task<Subject> SubjectEp(string subjectId)
        {
            return await $"{HOST}/subject/{subjectId}/ep"
                .GetAsync()
                .ReceiveJson<Subject>()
                .ContinueWith(t =>
                {
                    Subject subject = t.Result;
                    if (subject == null) return null;
                    // 将章节按类别排序
                    subject._eps = subject.Eps.OrderBy(c => c.Type).ThenBy(c => c.Sort).ToList();
                    foreach (var ep in subject.Eps)
                    {
                        ep.Name = System.Net.WebUtility.HtmlDecode(ep.Name);
                        ep.NameCn = ep.NameCn.IsNullOrEmpty() ? ep.Name : System.Net.WebUtility.HtmlDecode(ep.NameCn);
                    }
                    return _bgmCache.UpdateSubjectEp(subjectId, t.Result);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 获取指定条目收藏信息。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public async Task<SubjectStatus2> Status(string subjectId)
        {
            return await $"{HOST}/collection/{subjectId}"
                .GetAsync()
                .ReceiveJson<SubjectStatus2>()
                .ContinueWith(t => _bgmCache.UpdateStatus(subjectId, t.Result),
                    TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 获取指定条目收藏信息，批量。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, SubjectStatus>> Status(IEnumerable<string> subjectIds)
        {
            Dictionary<string, SubjectStatus> status = new Dictionary<string, SubjectStatus>();
            for (int i = 0; i < subjectIds.Count(); i += 20)
            {
                await $"{HOST}/user/{_bgmOAuth.MyToken.UserId}/collection"
                    .SetQueryParams(new
                    {
                        ids = string.Join(",", subjectIds.Skip(i).Take(20))
                    })
                    .GetAsync()
                    .ReceiveJson<Dictionary<string, SubjectStatus>>()
                    .ContinueWith(t =>
                    {
                        if (t.Result == null) return;
                        foreach (var item in t.Result)
                        {
                            status.Add(item.Key, item.Value);
                            _bgmCache.UpdateStatus(item.Key, item.Value);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            return status;
        }

        /// <summary>
        /// 获取用户指定条目收视进度。
        /// </summary>
        /// <param name="subject_id"></param>
        /// <returns></returns>
        public async Task<Progress> Progress(string subject_id)
        {
            return await $"{HOST}/user/{_bgmOAuth.MyToken.UserId}/progress"
                .SetQueryParams(new
                {
                    subject_id
                })
                .GetAsync()
                .ReceiveJson<Progress>()
                .ContinueWith(t => _bgmCache.UpdateProgress(subject_id, t.Result),
                    TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 更新指定条目收藏状态。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="collectionStatusEnum"></param>
        /// <param name="comment"></param>
        /// <param name="rating"></param>
        /// <param name="privacy"></param>
        /// <returns></returns>
        public async Task<SubjectStatus2> UpdateStatus(
            string subjectId, CollectionStatusEnum collectionStatusEnum,
            string comment = "", string rating = "", string privacy = "0")
        {
            return await $"{HOST}/collection/{subjectId}/update"
                .PostUrlEncodedAsync(new
                {
                    status = collectionStatusEnum.GetValue(),
                    comment,
                    rating,
                    privacy
                })
                .ReceiveJson<SubjectStatus2>()
                .ContinueWith(t => _bgmCache.UpdateStatus(subjectId, t.Result),
                    TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 更新收视进度。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<bool> UpdateProgress(string ep, EpStatusEnum status)
        {
            return await $"{HOST}/ep/{ep}/status/{status}"
                .GetAsync()
                .ReceiveString()
                .ContinueWith(t =>
                {
                    bool success = t.Result.Contains("\"error\":\"OK\"");
                    if (success)
                    {
                        _bgmCache.UpdateProgress(int.Parse(ep), status);
                    }
                    return success;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 批量更新收视进度。
        /// 使用 post 的 UrlEncoded 提交进行更新，更新收藏状态使用相同方法。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <param name="ep_id">章节id，逗号分隔</param>
        /// <returns></returns>
        public async Task<bool> UpdateProgressBatch(int ep, EpStatusEnum status, string ep_id)
        {
            return await $"{HOST}/ep/{ep}/status/{status}"
                .PostUrlEncodedAsync(new
                {
                    ep_id
                })
                .ReceiveString()
                .ContinueWith(t =>
                {
                    bool success = t.Result.Contains("\"error\":\"OK\"");
                    if (success)
                    {
                        foreach (var ep in ep_id.Split(','))
                        {
                            _bgmCache.UpdateProgress(int.Parse(ep), status);
                        }
                    }
                    return success;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 获取时间表。
        /// </summary>
        /// <returns></returns>
        public async Task<List<BangumiTimeLine>> Calendar()
        {
            return await $"{HOST}/calendar"
                .GetAsync()
                .ReceiveJson<List<BangumiTimeLine>>()
                .ContinueWith(t =>
                {
                    List<BangumiTimeLine> calendar = t.Result ?? new List<BangumiTimeLine>();
                    foreach (var bangumiCalendar in calendar)
                    {
                        foreach (var item in bangumiCalendar.Items)
                        {
                            item.Name = System.Net.WebUtility.HtmlDecode(item.Name);
                            item.NameCn = item.NameCn.IsNullOrEmpty() ? item.Name : System.Net.WebUtility.HtmlDecode(item.NameCn);
                            item.Images?.ConvertImageHttpToHttps();
                        }
                    }
                    return _bgmCache.UpdateCalendar(calendar);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
        /// <summary>
        /// 获取搜索结果。
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="type"></param>
        /// <param name="start"></param>
        /// <param name="max_results"></param>
        /// <returns></returns>
        public async Task<SearchResult> Search(string keyWord, string type, int start, int max_results)
        {
            var response = await $"{HOST}/search/subject/{keyWord}"
                .SetQueryParams(new
                {
                    type,
                    responseGroup = "small",
                    start,
                    max_results
                })
                .GetAsync()
                .ReceiveJson<SearchResult>();
            if (response?.Results == null)
            {
                return new SearchResult
                {
                    ResultCount = 0,
                    Results = new List<Subject>()
                };
            }
            foreach (var item in response.Results)
            {
                item.Name = System.Net.WebUtility.HtmlDecode(item.Name);
                item.NameCn = item.NameCn.IsNullOrEmpty() ? item.Name : System.Net.WebUtility.HtmlDecode(item.NameCn);
                item.Images?.ConvertImageHttpToHttps();
            }
            return response;
        }
    }
}
