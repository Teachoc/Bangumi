using Bangumi.Api.Exceptions;
using Bangumi.Api.Extensions;
using Bangumi.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace Bangumi.Api.Services
{
    internal class BangumiHttpWrapper
    {
        internal string BaseUrl { get; set; }
        internal string OAuthBaseUrl { get; set; }
        internal string ClientId { get; set; }
        internal string ClientSecret { get; set; }
        internal string RedirectUrl { get; set; }
        internal string NoImageUri { get; set; }

        /// <summary>
        /// 获取指定类别收藏信息。
        /// </summary>
        /// <param name="userIdString"></param>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        internal async Task<Collection2> GetSubjectCollectionAsync(string userIdString, SubjectTypeEnum subjectType)
        {
            try
            {
                var result = (await $"{BaseUrl}/user/{userIdString}/collections/{subjectType.GetValue()}"
                    .SetQueryParams(new
                    {
                        app_id = ClientId,
                        max_results = 25
                    })
                    .GetAsync()
                    .ReceiveJson<List<Collection2>>())
                    ?.FirstOrDefault()
                    ?? new Collection2 { Collects = new List<Collection>() };
                foreach (var type in result.Collects)
                {
                    foreach (var item in type.Items)
                    {
                        item.Subject.Name = System.Net.WebUtility.HtmlDecode(item.Subject.Name);
                        item.Subject.NameCn = item.Subject.NameCn.IsNullOrEmpty() ? item.Subject.Name : System.Net.WebUtility.HtmlDecode(item.Subject.NameCn);
                        if (item.Subject.Images == null)
                        {
                            item.Subject.Images = new Images
                            {
                                Grid = NoImageUri,
                                Small = NoImageUri,
                                Common = NoImageUri,
                                Medium = NoImageUri,
                                Large = NoImageUri,
                            };
                        }
                        else
                        {
                            item.Subject.Images.ConvertImageHttpToHttps();
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetSubjectCollectionAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 获取指定条目收藏信息。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        internal async Task<SubjectStatus2> GetCollectionStatusAsync(string access_token, string subjectId)
        {
            try
            {
                return await $"{BaseUrl}/collection/{subjectId}"
                    .SetQueryParams(new
                    {
                        access_token
                    })
                    .GetAsync()
                    .ReceiveJson<SubjectStatus2>();
            }
            catch (Exception)
            {
                Debug.WriteLine("GetCollectionStatusAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 获取用户指定条目收视进度。
        /// </summary>
        /// <param name="userIdString"></param>
        /// <param name="access_token"></param>
        /// <param name="subject_id"></param>
        /// <returns></returns>
        internal async Task<Progress> GetProgressesAsync(string userIdString, string access_token, string subject_id)
        {
            try
            {
                return await $"{BaseUrl}/user/{userIdString}/progress"
                    .SetQueryParams(new
                    {
                        subject_id,
                        access_token
                    })
                    .GetAsync()
                    .ReceiveJson<Progress>();
            }
            catch (Exception)
            {
                Debug.WriteLine("GetProgressesAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 获取用户收视列表。
        /// </summary>
        /// <param name="userIdString"></param>
        /// <returns></returns>
        internal async Task<List<Watching>> GetWatchingListAsync(string userIdString)
        {
            try
            {
                var result = (await $"{BaseUrl}/user/{userIdString}/collection"
                    .SetQueryParams(new
                    {
                        cat = "watching",
                        responseGroup = "medium"
                    })
                    .GetAsync()
                    .ReceiveJson<List<Watching>>())
                    ?? new List<Watching>();
                foreach (var watching in result)
                {
                    watching.Subject.Name = System.Net.WebUtility.HtmlDecode(watching.Subject.Name);
                    watching.Subject.NameCn = watching.Subject.NameCn.IsNullOrEmpty() ?
                        watching.Subject.Name : System.Net.WebUtility.HtmlDecode(watching.Subject.NameCn);
                    if (watching.Subject.Images == null)
                    {
                        watching.Subject.Images = new Images
                        {
                            Grid = NoImageUri,
                            Small = NoImageUri,
                            Common = NoImageUri,
                            Medium = NoImageUri,
                            Large = NoImageUri,
                        };
                    }
                    else
                    {
                        watching.Subject.Images.ConvertImageHttpToHttps();
                    }
                }
                return result;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetWatchingListAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 更新指定条目收藏状态。
        /// </summary>
        /// <param name="accessTokenString"></param>
        /// <param name="subjectId"></param>
        /// <param name="collectionStatusEnum"></param>
        /// <param name="comment"></param>
        /// <param name="rating"></param>
        /// <param name="privacy"></param>
        /// <returns></returns>
        internal async Task<bool> UpdateCollectionStatusAsync(string access_token,
                                                                   string subjectId,
                                                                   CollectionStatusEnum collectionStatusEnum,
                                                                   string comment = "",
                                                                   string rating = "",
                                                                   string privacy = "0")
        {
            try
            {
                var response = await $"{BaseUrl}/collection/{subjectId}/update"
                    .SetQueryParams(new
                    {
                        access_token
                    })
                    .PostUrlEncodedAsync(new
                    {
                        status = collectionStatusEnum.GetValue(),
                        comment,
                        rating,
                        privacy
                    })
                    .ReceiveString();
                if (response.Contains($"\"type\":\"{collectionStatusEnum.GetValue()}\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine("UpdateCollectionStatusAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 更新收视进度。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal async Task<bool> UpdateProgressAsync(string access_token, string ep, EpStatusEnum status)
        {
            try
            {
                var response = await $"{BaseUrl}/ep/{ep}/status/{status}"
                    .SetQueryParams(new
                    {
                        access_token
                    })
                    .GetAsync()
                    .ReceiveString();
                if (response.Contains("\"error\":\"OK\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine("UpdateProgressAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 批量更新收视进度。
        /// 使用 post 的 UrlEncoded 提交进行更新，更新收藏状态使用相同方法。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <param name="ep_id">章节id，逗号分隔</param>
        /// <returns></returns>
        internal async Task<bool> UpdateProgressBatchAsync(string access_token, int ep, EpStatusEnum status, string ep_id)
        {
            try
            {
                var response = await $"{BaseUrl}/ep/{ep}/status/{status}"
                    .SetQueryParams(new
                    {
                        access_token
                    })
                    .PostUrlEncodedAsync(new
                    {
                        ep_id
                    })
                    .ReceiveString();
                if (response.Contains("\"error\":\"OK\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine("UpdateProgressBatchAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 获取指定条目所有章节。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        internal async Task<Subject> GetSubjectEpsAsync(string subjectId)
        {
            try
            {
                var result = await $"{BaseUrl}/subject/{subjectId}/ep"
                    .GetAsync()
                    .ReceiveJson<Subject>();
                if (result == null) return null;
                result.Name = System.Net.WebUtility.HtmlDecode(result.Name);
                result.NameCn = result.NameCn.IsNullOrEmpty() ? result.Name : System.Net.WebUtility.HtmlDecode(result.NameCn);
                // 将章节按类别排序
                result._eps = result.Eps.OrderBy(c => c.Type).ThenBy(c => c.Sort).ToList();
                foreach (var ep in result.Eps)
                {
                    ep.Name = System.Net.WebUtility.HtmlDecode(ep.Name);
                    ep.NameCn = ep.NameCn.IsNullOrEmpty() ? ep.Name : System.Net.WebUtility.HtmlDecode(ep.NameCn);
                }

                return result;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetSubjectEpsAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 获取指定条目详情。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        internal async Task<Subject> GetSubjectAsync(string subjectId)
        {
            try
            {
                var result = await $"{BaseUrl}/subject/{subjectId}?responseGroup=large"
                    .GetAsync()
                    .ReceiveJson<Subject>();
                if (result == null) return null;
                result.Name = System.Net.WebUtility.HtmlDecode(result.Name);
                result.NameCn = System.Net.WebUtility.HtmlDecode(result.NameCn);
                if (result.Images == null)
                {
                    result.Images = new Images
                    {
                        Grid = NoImageUri,
                        Small = NoImageUri,
                        Common = NoImageUri,
                        Medium = NoImageUri,
                        Large = NoImageUri,
                    };
                }
                else
                {
                    result.Images.ConvertImageHttpToHttps();
                }
                // 将章节按类别排序
                result._eps = result.Eps.OrderBy(c => c.Type).ThenBy(c => c.Sort).ToList();
                foreach (var ep in result.Eps)
                {
                    ep.Name = System.Net.WebUtility.HtmlDecode(ep.Name);
                    ep.NameCn = ep.NameCn.IsNullOrEmpty() ? ep.Name : System.Net.WebUtility.HtmlDecode(ep.NameCn);
                }
                if (result.Blogs != null)
                {
                    foreach (var blog in result.Blogs)
                    {
                        blog.Title = System.Net.WebUtility.HtmlDecode(blog.Title);
                        blog.Summary = System.Net.WebUtility.HtmlDecode(blog.Summary);
                    }
                }
                if (result.Topics != null)
                {
                    foreach (var topic in result.Topics)
                    {
                        topic.Title = System.Net.WebUtility.HtmlDecode(topic.Title);
                    }
                }
                return result;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetSubjectAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 获取时间表。
        /// </summary>
        /// <returns></returns>
        internal async Task<List<BangumiTimeLine>> GetBangumiCalendarAsync()
        {
            try
            {
                var result = await $"{BaseUrl}/calendar"
                    .GetAsync()
                    .ReceiveJson<List<BangumiTimeLine>>();
                if (result == null) return null;
                foreach (var bangumiCalendar in result)
                {
                    foreach (var item in bangumiCalendar.Items)
                    {
                        item.Name = System.Net.WebUtility.HtmlDecode(item.Name);
                        item.NameCn = item.NameCn.IsNullOrEmpty() ? item.Name : System.Net.WebUtility.HtmlDecode(item.NameCn);
                        if (item.Images == null)
                        {
                            item.Images = new Images
                            {
                                Grid = NoImageUri,
                                Small = NoImageUri,
                                Common = NoImageUri,
                                Medium = NoImageUri,
                                Large = NoImageUri,
                            };
                        }
                        else
                        {
                            item.Images.ConvertImageHttpToHttps();
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetBangumiCalendarAsync Error.");
                throw;
            }
        }
        /// <summary>
        /// 获取搜索结果。
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="type"></param>
        /// <param name="start"></param>
        /// <param name="max_results"></param>
        /// <returns></returns>
        internal async Task<SearchResult> GetSearchResultAsync(string keyWord, string type, int start, int max_results)
        {
            try
            {
                var result = await $"{BaseUrl}/search/subject/{keyWord}"
                    .SetQueryParams(new
                    {
                        type,
                        responseGroup = "small",
                        start,
                        max_results
                    })
                    .GetAsync()
                    .ReceiveJson<SearchResult>();
                if (result?.Results == null)
                {
                    return new SearchResult
                    {
                        ResultCount = 0,
                        Results = new List<Subject>()
                    };
                }
                foreach (var item in result.Results)
                {
                    item.Name = System.Net.WebUtility.HtmlDecode(item.Name);
                    item.NameCn = item.NameCn.IsNullOrEmpty() ? item.Name : System.Net.WebUtility.HtmlDecode(item.NameCn);
                    if (item.Images == null)
                    {
                        item.Images = new Images
                        {
                            Grid = NoImageUri,
                            Small = NoImageUri,
                            Common = NoImageUri,
                            Medium = NoImageUri,
                            Large = NoImageUri,
                        };
                    }
                    else
                    {
                        item.Images.ConvertImageHttpToHttps();
                    }
                }
                return result;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetSearchResultAsync Error.");
                throw;
            }
        }

        /// <summary>
        /// 使用 code 换取 Access Token。
        /// </summary>
        /// <param name="code"></param>
        /// <returns>获取失败返回 null。</returns>
        internal async Task<AccessToken> GetTokenAsync(string code)
        {
            try
            {
                return await $"{OAuthBaseUrl}/access_token"
                    .PostUrlEncodedAsync(new
                    {
                        grant_type = "authorization_code",
                        client_id = ClientId,
                        client_secret = ClientSecret,
                        code,
                        redirect_uri = RedirectUrl
                    })
                    .ReceiveJson<AccessToken>();
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetAccessToken Error.");
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// 刷新授权有效期。
        /// </summary>
        /// <param name="token"></param>
        /// <returns>获取失败返回 null。</returns>
        private async Task<AccessToken> RefreshTokenAsync(AccessToken token)
        {
            try
            {
                return await $"{OAuthBaseUrl}/access_token"
                    .PostUrlEncodedAsync(new
                    {
                        grant_type = "refresh_token",
                        client_id = ClientId,
                        client_secret = ClientSecret,
                        refresh_token = token.RefreshToken,
                        redirect_uri = RedirectUrl
                    })
                    .ReceiveJson<AccessToken>();
            }
            catch (BgmUnauthorizedException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.WriteLine("RefreshAccessToken Error.");
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// 查询授权信息，并在满足条件时刷新Token。
        /// </summary>
        /// <param name="token"></param>
        /// <returns>获取失败返回 null，可能会抛出异常。</returns>
        internal async Task<AccessToken> CheckTokenAsync(AccessToken token)
        {
            try
            {
                var result = await $"{OAuthBaseUrl}/token_status"
                    .SetQueryParams(new
                    {
                        access_token = token.Token
                    })
                    .PostStringAsync(string.Empty)
                    .ReceiveJson<AccessToken>();
                // 获取4天后的时间戳，离过期不足4天时或过期后更新 access_token
                if (result.Expires < DateTime.Now.AddDays(4).ConvertDateTimeToJsTick())
                    return await RefreshTokenAsync(token);
                return token;
            }
            catch (BgmUnauthorizedException)
            {
                return await RefreshTokenAsync(token);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }


    }
}
