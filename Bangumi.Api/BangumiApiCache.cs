using Bangumi.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Bangumi.Api
{
    /// <summary>
    /// BangumiApi 的 Cache 部分
    /// </summary>
    public static partial class BangumiApi
    {
        /// <summary>
        /// 缓存文件夹路径
        /// </summary>
        private static string _cacheFolderPath;

        /// <summary>
        /// 记录缓存是否更新过
        /// </summary>
        private static bool _isCacheUpdated;

        /// <summary>
        /// 定时器
        /// </summary>
        private static Timer _timer;

        /// <summary>
        /// 定时器触发间隔
        /// </summary>
        private const int TimerInterval = 30000;

        /// <summary>
        /// 缓存今天是否更新
        /// </summary>
        public static bool IsCacheUpdatedToday
        {
            get => BangumiCache.UpdateDate == DateTime.Today.ConvertDateTimeToJsTick();
            set
            {
                _isCacheUpdated = false;
                BangumiCache.UpdateDate = (value ? DateTime.Today : DateTime.Today.AddDays(-1)).ConvertDateTimeToJsTick();
                _isCacheUpdated = true;
            }
        }

        /// <summary>
        /// 存储缓存数据
        /// </summary>
        public static BangumiCache BangumiCache { get; private set; }


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
        /// 清空缓存并删除缓存文件
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
            _isCacheUpdated = false;
            if (collectionStatus != CollectionStatusEnum.No)
            {
                BangumiCache.SubjectStatus.TryGetValue(subjectId, out var status);
                if (status != null)
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
                    BangumiCache.SubjectStatus.Add(subjectId,
                        new SubjectStatus2()
                        {
                            Status = new SubjectStatus()
                            {
                                Id = (int)collectionStatus,
                                Type = collectionStatus.GetValue(),
                            },
                            Comment = comment,
                            Rating = string.IsNullOrEmpty(rating) ? 0 : int.Parse(rating),
                            Private = privace,
                        });
                }
                // 若状态不是在做，则从进度中删除
                if (collectionStatus != CollectionStatusEnum.Do)
                {
                    BangumiCache.Watchings.RemoveAll(w => w.SubjectId.ToString() == subjectId);
                }
            }
            else
            {
                BangumiCache.SubjectStatus.Remove(subjectId);
                BangumiCache.Watchings.RemoveAll(w => w.SubjectId.ToString() == subjectId);
            }
            _isCacheUpdated = true;
        }

        /// <summary>
        /// 更新缓存记录的章节状态
        /// </summary>
        /// <param name="epId"></param>
        /// <param name="status"></param>
        private static void UpdateProgressCache(int epId, EpStatusEnum status)
        {
            // 找到该章节所属的条目
            var sub = BangumiCache.Subjects.Values.FirstOrDefault(s => s.Eps.FirstOrDefault(p => p.Id == epId) != null);
            if (sub != null)
            {
                _isCacheUpdated = false;
                // 找到已有进度，否则新建
                BangumiCache.Progresses.TryGetValue(sub.Id.ToString(), out var pro);
                if (pro != null)
                {
                    var ep = pro.Eps.FirstOrDefault(e => e.Id == epId);
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
                var watch = BangumiCache.Watchings.FirstOrDefault(w => w.SubjectId == sub.Id);
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
                if (dic[key].EqualsExT(value)) return;

                _isCacheUpdated = false;
                dic[key] = value;
                _isCacheUpdated = true;
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
            if (source.SequenceEqualExT(dest)) return;

            _isCacheUpdated = false;
            source = dest;
            _isCacheUpdated = true;
        }
        #endregion

    }
}
