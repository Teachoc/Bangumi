using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class BangumiCache
    {
        /// <summary>
        /// 收藏，含动画、书籍、音乐、游戏、三次元
        /// </summary>
        public ConcurrentDictionary<string, Collection2> Collections { get; set; } = new ConcurrentDictionary<string, Collection2>();

        /// <summary>
        /// 条目详情
        /// </summary>
        public ConcurrentDictionary<string, Subject> Subjects { get; set; } = new ConcurrentDictionary<string, Subject>();

        /// <summary>
        /// 条目收藏信息
        /// </summary>
        public ConcurrentDictionary<string, SubjectStatus2> SubjectStatus { get; set; } = new ConcurrentDictionary<string, SubjectStatus2>();

        /// <summary>
        /// 条目收视进度
        /// </summary>
        public ConcurrentDictionary<string, Progress> Progresses { get; set; } = new ConcurrentDictionary<string, Progress>();

        /// <summary>
        /// 用户收视列表
        /// </summary>
        public List<Watching> Watchings { get; set; } = new List<Watching>();

        /// <summary>
        /// 时间表
        /// </summary>
        public List<BangumiTimeLine> TimeLine { get; set; } = new List<BangumiTimeLine>();

        /// <summary>
        /// 缓存更新时间
        /// </summary>
        public long UpdateDate { get; set; }
    }
}
