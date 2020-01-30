using Bangumi.Api.Common;
using Bangumi.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Bangumi.Api.Services
{
    public class BgmCache : IBgmCache
    {
        /// <summary>
        /// 缓存文件夹路径
        /// </summary>
        private readonly string _cacheFolder;

        /// <summary>
        /// 记录缓存是否更新过
        /// </summary>
        private bool _isCacheUpdated;

        /// <summary>
        /// 定时器
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// 定时器触发间隔
        /// </summary>
        private const int TimerInterval = 30000;
        private Cache _cache;

        public BgmCache(string cacheFolder)
        {
            _cacheFolder = cacheFolder ?? throw new ArgumentNullException(nameof(cacheFolder));
            // 加载缓存
            if (File.Exists(AppFile.BangumiCache.GetFilePath(_cacheFolder)))
            {
                try
                {
                    Task.Run(async () =>
                    {
                        _cache = JsonConvert.DeserializeObject<Cache>(await FileHelper.ReadTextAsync(AppFile.BangumiCache.GetFilePath(_cacheFolder)));
                    }).Wait();
                }
                catch (Exception)
                {
                    FileHelper.DeleteFile(AppFile.BangumiCache.GetFilePath(_cacheFolder));
                    _cache = new Cache();
                }
            }
            _cache ??= new Cache();
            // 启动定时器，定时将缓存写入文件，30 秒
            _timer = new Timer(TimerInterval);
            _timer.Elapsed += WriteCacheToFileTimer_Elapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public bool IsUpdatedToday
        {
            get => _cache.UpdateDate == DateTime.Today.ToJsTick();
            set
            {
                if (IsUpdatedToday != value)
                {
                    _isCacheUpdated = false;
                    _cache.UpdateDate = (value ? DateTime.Today : DateTime.Today.AddDays(-1)).ToJsTick();
                    _isCacheUpdated = true;
                }
            }
        }

        /// <summary>
        /// 定时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WriteCacheToFileTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await WriteToFile();
        }

        public async Task WriteToFile()
        {
            if (_isCacheUpdated)
            {
                _isCacheUpdated = false;
                await FileHelper.WriteTextAsync(AppFile.BangumiCache.GetFilePath(_cacheFolder),
                                                JsonConvert.SerializeObject(_cache));
            }
        }

        public void Delete()
        {
            _isCacheUpdated = false;
            _cache = null;
            _cache = new Cache();
            FileHelper.DeleteFile(AppFile.BangumiCache.GetFilePath(_cacheFolder));
        }

        public long GetFileLength()
        {
            return FileHelper.GetFileLength(AppFile.BangumiCache.GetFilePath(_cacheFolder));
        }




        List<Watching> IBgmCache.UpdateWatching(List<Watching> value)
        {
            _cache.Watching = value;
            _isCacheUpdated = true;
            return _cache.Watching;
        }

        CollectionE IBgmCache.UpdateCollections(SubjectType key, CollectionE value)
        {
            return _cache.Collections.AddOrUpdate(key.GetValue(), value, (k, v) =>
            {
                if (!v.EqualsExT(value))
                    _isCacheUpdated = true;
                return value;
            });
        }

        SubjectLarge IBgmCache.UpdateSubject(string key, SubjectLarge value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return _cache.Subject.AddOrUpdate(key, value, (k, v) =>
            {
                if (!v.EqualsExT(value))
                    _isCacheUpdated = true;
                return value;
            });
        }

        SubjectLarge IBgmCache.UpdateSubjectEp(string subjectId, SubjectLarge subject)
        {
            var sub = _cache.Subject.GetOrAdd(subjectId, subject);
            if (sub == null)
            {
                return subject;
            }
            if (!sub.Eps.SequenceEqualExT(subject.Eps))
            {
                sub.Eps = subject.Eps;
                _isCacheUpdated = true;
            }
            return sub;
        }

        CollectionStatusE IBgmCache.UpdateStatus(string subjectId, CollectionStatusE subjectStatus)
        {
            if (subjectStatus != null)
            {
                _cache.Status.AddOrUpdate(subjectId, subjectStatus, (key, value) => subjectStatus);
                // 若状态不是在做，则从进度中删除
                if (subjectStatus.Status?.Id != CollectionStatusType.Do)
                {
                    _cache.Watching.RemoveAll(w => w.SubjectId.ToString() == subjectId);
                }
                else if (subjectStatus.Status?.Id == CollectionStatusType.Do &&
                         _cache.Watching.All(w => w.SubjectId.ToString() != subjectId) &&
                         _cache.Subject.TryGetValue(subjectId, out var subject) &&
                         subject.Type == SubjectType.Anime)
                {
                    _cache.Watching.Add(new Watching
                    {
                        SubjectId = subject.Id,
                        LastTouch = DateTime.Now.ToJsTick(),
                        Name = subject.Name,
                        Subject = new SubjectForWatching
                        {
                            Id = subject.Id,
                            Name = subject.Name,
                            NameCn = subject.NameCn,
                            Type = subject.Type,
                            AirDate = subject.AirDate,
                            AirWeekday = subject.AirWeekday,
                            EpsCount = subject.EpsCount,
                            Images = subject.Images,
                            Url = subject.Url
                        }
                    });
                }
            }
            else
            {
                // 若未收藏，则删除收藏状态，且从进度中删除
                _cache.Status.TryRemove(subjectId, out _);
                _cache.Watching.RemoveAll(w => w.SubjectId.ToString() == subjectId);
            }
            _isCacheUpdated = true;
            return subjectStatus;
        }

        CollectionStatus IBgmCache.UpdateStatus(string subjectId, CollectionStatus subjectStatus)
        {
            if (subjectStatus != null)
            {
                _cache.Status.AddOrUpdate(subjectId, new CollectionStatusE { Status = subjectStatus }, (key, value) =>
                {
                    value.Status = subjectStatus;
                    return value;
                });
                _isCacheUpdated = true;
            }
            return subjectStatus;
        }

        Progress IBgmCache.UpdateProgress(string key, Progress value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return _cache.Progress.AddOrUpdate(key, value, (k, v) =>
            {
                if (!v.EqualsExT(value))
                    _isCacheUpdated = true;
                return value;
            });
        }

        void IBgmCache.UpdateProgress(int epId, EpStatusType status)
        {
            // 找到该章节所属的条目
            var sub = _cache.Subject.Values.FirstOrDefault(s => s.Eps?.FirstOrDefault(p => p.Id == epId) != null);
            if (sub != null)
            {
                // 找到已有进度，否则新建
                _cache.Progress.TryGetValue(sub.Id.ToString(), out var pro);
                if (pro != null)
                {
                    var ep = pro.Eps.FirstOrDefault(e => e.Id == epId);
                    if (status != EpStatusType.remove)
                    {
                        if (ep != null)
                        {
                            ep.Status.Id = status;
                            ep.Status.CnName = status.GetCnName();
                            ep.Status.CssName = status.GetCssName();
                            ep.Status.UrlName = status.GetUrlName();
                        }
                        else
                        {
                            pro.Eps.Add(new EpStatusE()
                            {
                                Id = epId,
                                Status = new EpStatus()
                                {
                                    Id = status,
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
                else if (status != EpStatusType.remove)
                {
                    var progress = new Progress()
                    {
                        SubjectId = sub.Id,
                        Eps = new List<EpStatusE>()
                        {
                            new EpStatusE()
                            {
                                Id = epId,
                                Status = new EpStatus()
                                {
                                    Id = status,
                                    CnName = status.GetCnName(),
                                    CssName = status.GetCssName(),
                                    UrlName = status.GetUrlName(),
                                }
                            }
                        }
                    };
                    _cache.Progress.AddOrUpdate(sub.Id.ToString(), progress, (k, v) => progress);
                }
                // 找到收视列表中的条目，修改 LastTouch
                var watch = _cache.Watching.FirstOrDefault(w => w.SubjectId == sub.Id);
                if (watch != null)
                {
                    watch.LastTouch = DateTime.Now.ToJsTick();
                }
                _isCacheUpdated = true;
            }
        }

        List<Calendar> IBgmCache.UpdateCalendar(List<Calendar> value)
        {
            _cache.Calendar = value;
            _isCacheUpdated = true;
            return _cache.Calendar;
        }



        public List<Watching> Watching()
        {
            return _cache.Watching;
        }

        public CollectionE Collections(SubjectType key)
        {
            _cache.Collections.TryGetValue(key.GetValue(), out var value);
            return value;
        }

        public SubjectLarge Subject(string key)
        {
            _cache.Subject.TryGetValue(key, out var value);
            return value;
        }

        public CollectionStatusE Status(string key)
        {
            _cache.Status.TryGetValue(key, out var value);
            return value;
        }

        public Progress Progress(string key)
        {
            _cache.Progress.TryGetValue(key, out var value);
            return value;
        }

        public List<Calendar> Calendar()
        {
            return _cache.Calendar;
        }
    }

    public class Cache
    {
        /// <summary>
        /// 缓存更新时间
        /// </summary>
        public int UpdateDate { get; set; }

        /// <summary>
        /// 用户收视列表
        /// </summary>
        public List<Watching> Watching { get; set; } = new List<Watching>();

        /// <summary>
        /// 收藏，含动画、书籍、音乐、游戏、三次元
        /// </summary>
        public ConcurrentDictionary<string, CollectionE> Collections { get; set; } = new ConcurrentDictionary<string, CollectionE>();

        /// <summary>
        /// 条目详情
        /// </summary>
        public ConcurrentDictionary<string, SubjectLarge> Subject { get; set; } = new ConcurrentDictionary<string, SubjectLarge>();

        /// <summary>
        /// 条目收藏信息
        /// </summary>
        public ConcurrentDictionary<string, CollectionStatusE> Status { get; set; } = new ConcurrentDictionary<string, CollectionStatusE>();

        /// <summary>
        /// 条目收视进度
        /// </summary>
        public ConcurrentDictionary<string, Progress> Progress { get; set; } = new ConcurrentDictionary<string, Progress>();

        /// <summary>
        /// 时间表
        /// </summary>
        public List<Calendar> Calendar { get; set; } = new List<Calendar>();

    }
}
