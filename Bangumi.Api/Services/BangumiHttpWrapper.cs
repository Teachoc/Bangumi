using Bangumi.Api.Models;
using Bangumi.Api.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
            string url = string.Format("{0}/user/{1}/collections/{2}?app_id={3}&max_results=25", BaseUrl, userIdString, subjectType.GetValue(), ClientId);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response == "null")
                {
                    return new Collection2 { Collects = new List<Collection>() };
                }
                var result = JsonConvert.DeserializeObject<List<Collection2>>(response);
                foreach (var type in result.ElementAt(0).Collects)
                {
                    foreach (var item in type.Items)
                    {
                        item.Subject.Name = System.Net.WebUtility.HtmlDecode(item.Subject.Name);
                        item.Subject.NameCn = string.IsNullOrEmpty(item.Subject.NameCn) ? item.Subject.Name : System.Net.WebUtility.HtmlDecode(item.Subject.NameCn);
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
                return result.ElementAt(0);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectCollectionAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目收藏信息。
        /// </summary>
        /// <param name="accessTokenString"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        internal async Task<SubjectStatus2> GetCollectionStatusAsync(string accessTokenString, string subjectId)
        {
            string url = string.Format("{0}/collection/{1}?access_token={2}", BaseUrl, subjectId, accessTokenString);

            try
            {
                string response = await HttpHelper.GetAsync(url);
                return JsonConvert.DeserializeObject<SubjectStatus2>(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetCollectionStatusAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取用户指定条目收视进度。
        /// </summary>
        /// <param name="userIdString"></param>
        /// <param name="accessTokenString"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        internal async Task<Progress> GetProgressesAsync(string userIdString, string accessTokenString, string subjectId)
        {
            string url = string.Format("{0}/user/{1}/progress?subject_id={2}&access_token={3}", BaseUrl, userIdString, subjectId, accessTokenString);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                return JsonConvert.DeserializeObject<Progress>(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetProgressesAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取用户收视列表。
        /// </summary>
        /// <param name="userIdString"></param>
        /// <returns></returns>
        internal async Task<List<Watching>> GetWatchingListAsync(string userIdString)
        {
            string url = string.Format("{0}/user/{1}/collection?cat=watching&responseGroup=medium", BaseUrl, userIdString);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response == "null")
                {
                    return new List<Watching>();
                }
                var result = JsonConvert.DeserializeObject<List<Watching>>(response);
                foreach (var watching in result)
                {
                    watching.Subject.Name = System.Net.WebUtility.HtmlDecode(watching.Subject.Name);
                    watching.Subject.NameCn = string.IsNullOrEmpty(watching.Subject.NameCn) ?
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
            catch (Exception e)
            {
                Debug.WriteLine("GetWatchingListAsync Error.");
                throw e;
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
        /// <param name="privace"></param>
        /// <returns></returns>
        internal async Task<bool> UpdateCollectionStatusAsync(string accessTokenString,
                                                                   string subjectId,
                                                                   CollectionStatusEnum collectionStatusEnum,
                                                                   string comment = "",
                                                                   string rating = "",
                                                                   string privace = "0")
        {
            string url = string.Format("{0}/collection/{1}/update?access_token={2}", BaseUrl, subjectId, accessTokenString);
            string postData = "status=" + collectionStatusEnum.GetValue();
            postData += "&comment=" + System.Net.WebUtility.UrlEncode(comment);
            postData += "&rating=" + rating;
            postData += "&privacy=" + privace;

            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                if (response.Contains(string.Format("\"type\":\"{0}\"", collectionStatusEnum.GetValue())))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateCollectionStatusAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 更新收视进度。
        /// </summary>
        /// <param name="accessTokenString"></param>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal async Task<bool> UpdateProgressAsync(string accessTokenString, string ep, EpStatusEnum status)
        {
            string url = string.Format("{0}/ep/{1}/status/{2}?access_token={3}", BaseUrl, ep, status, accessTokenString);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response.Contains("\"error\":\"OK\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 批量更新收视进度。
        /// 使用 HttpWebRequest 提交表单进行更新，更新收藏状态使用相同方法。
        /// </summary>
        /// <param name="accessTokenString"></param>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <param name="epsId"></param>
        /// <returns></returns>
        internal async Task<bool> UpdateProgressBatchAsync(string accessTokenString, int ep, EpStatusEnum status, string epsId)
        {
            string url = string.Format("{0}/ep/{1}/status/{2}?access_token={3}", BaseUrl, ep, status, accessTokenString);
            string postData = "ep_id=" + epsId;

            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                if (response.Contains("\"error\":\"OK\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressBatchAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目所有章节。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        internal async Task<Subject> GetSubjectEpsAsync(string subjectId)
        {
            string url = string.Format("{0}/subject/{1}/ep", BaseUrl, subjectId);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                var result = JsonConvert.DeserializeObject<Subject>(response);
                result.Name = System.Net.WebUtility.HtmlDecode(result.Name);
                result.NameCn = string.IsNullOrEmpty(result.NameCn) ? result.Name : System.Net.WebUtility.HtmlDecode(result.NameCn);
                if (result.Eps != null)
                {
                    // 将章节按类别排序
                    result.Eps = result.Eps.OrderBy(c => c.Type).ToList();
                    foreach (var ep in result.Eps)
                    {
                        ep.Name = System.Net.WebUtility.HtmlDecode(ep.Name);
                        ep.NameCn = string.IsNullOrEmpty(ep.NameCn) ? ep.Name : System.Net.WebUtility.HtmlDecode(ep.NameCn);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectEpsAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目详情。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        internal async Task<Subject> GetSubjectAsync(string subjectId)
        {
            string url = string.Format("{0}/subject/{1}?responseGroup=large", BaseUrl, subjectId);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                var result = JsonConvert.DeserializeObject<Subject>(response);
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
                if (result.Eps != null)
                {
                    // 将章节按类别排序
                    result.Eps = result.Eps.OrderBy(c => c.Type).ToList();
                    foreach (var ep in result.Eps)
                    {
                        ep.Name = System.Net.WebUtility.HtmlDecode(ep.Name);
                        ep.NameCn = string.IsNullOrEmpty(ep.NameCn) ? ep.Name : System.Net.WebUtility.HtmlDecode(ep.NameCn);
                    }
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
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取时间表。
        /// </summary>
        /// <returns></returns>
        internal async Task<List<BangumiTimeLine>> GetBangumiCalendarAsync()
        {
            string url = string.Format("{0}/calendar", BaseUrl);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                var result = JsonConvert.DeserializeObject<List<BangumiTimeLine>>(response);
                foreach (var bangumiCalendar in result)
                {
                    foreach (var item in bangumiCalendar.Items)
                    {
                        item.Name = System.Net.WebUtility.HtmlDecode(item.Name);
                        item.NameCn = string.IsNullOrEmpty(item.NameCn) ? item.Name : System.Net.WebUtility.HtmlDecode(item.NameCn);
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
            catch (Exception e)
            {
                Debug.WriteLine("GetBangumiCalendarAsync Error.");
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
        internal async Task<SearchResult> GetSearchResultAsync(string keyWord, string type, int start, int n)
        {
            string url = string.Format("{0}/search/subject/{1}?type={2}&responseGroup=small&start={3}&max_results={4}", BaseUrl, keyWord, type, start, n);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (!response.StartsWith("<!DOCTYPE html>"))
                {
                    var result = JsonConvert.DeserializeObject<SearchResult>(response);
                    if (result != null && result.Results != null)
                    {
                        foreach (var item in result.Results)
                        {
                            item.Name = System.Net.WebUtility.HtmlDecode(item.Name);
                            item.NameCn = string.IsNullOrEmpty(item.NameCn) ? item.Name : System.Net.WebUtility.HtmlDecode(item.NameCn);
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
                }
                return new SearchResult { ResultCount = 0, Results = new List<Subject>() };
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSearchResultAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 使用 code 换取 Access Token。
        /// </summary>
        /// <param name="code"></param>
        /// <returns>获取失败返回 null。</returns>
        internal async Task<AccessToken> GetTokenAsync(string code)
        {
            string url = $"{OAuthBaseUrl}/access_token";
            string postData = "grant_type=authorization_code";
            postData += "&client_id=" + ClientId;
            postData += "&client_secret=" + ClientSecret;
            postData += "&code=" + code;
            postData += "&redirect_uri=" + RedirectUrl;
            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                return JsonConvert.DeserializeObject<AccessToken>(response);
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
        internal async Task<AccessToken> RefreshTokenAsync(AccessToken token)
        {
            string url = $"{OAuthBaseUrl}/access_token";
            string postData = "grant_type=refresh_token";
            postData += "&client_id=" + ClientId;
            postData += "&client_secret=" + ClientSecret;
            postData += "&refresh_token=" + token.RefreshToken;
            postData += "&redirect_uri=" + RedirectUrl;
            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                return JsonConvert.DeserializeObject<AccessToken>(response);
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
            string url = string.Format("{0}/token_status?access_token={1}", OAuthBaseUrl, token.Token);

            try
            {
                string response = await HttpHelper.PostAsync(url);
                var result = JsonConvert.DeserializeObject<AccessToken>(response);
                // 获取4天后的时间戳，离过期不足4天时或过期后更新 access_token
                if (result.Expires < DateTime.Now.AddDays(4).ConvertDateTimeToJsTick())
                    return await RefreshTokenAsync(token);
                return token;
            }
            catch (Exception e)
            {
                if (e.Message.Equals("401"))
                {
                    return await RefreshTokenAsync(token);
                }
                Debug.WriteLine(e.Message);
                throw e;
            }
        }


    }
}
