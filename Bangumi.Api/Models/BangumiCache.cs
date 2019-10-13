using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class BangumiCache
    {
        internal Dictionary<string, Collection2> _collections;
        /// <summary>
        /// 收藏，含动画、书籍、音乐、游戏、三次元
        /// </summary>
        public Dictionary<string, Collection2> Collections
        {
            get
            {
                if (_collections == null)
                {
                    _collections = new Dictionary<string, Collection2>();
                }
                return _collections;
            }
        }

        internal Dictionary<string, Subject> _subjects;
        /// <summary>
        /// 条目详情
        /// </summary>
        public Dictionary<string, Subject> Subjects
        {
            get
            {
                if (_subjects == null)
                {
                    _subjects = new Dictionary<string, Subject>();
                }
                return _subjects;
            }
        }

        internal Dictionary<string, SubjectStatus2> _subjectStatus;
        /// <summary>
        /// 条目收藏信息
        /// </summary>
        public Dictionary<string, SubjectStatus2> SubjectStatus
        {
            get
            {
                if (_subjectStatus == null)
                {
                    _subjectStatus = new Dictionary<string, SubjectStatus2>();
                }
                return _subjectStatus;
            }
        }

        internal Dictionary<string, Progress> _progresses;
        /// <summary>
        /// 条目收视进度
        /// </summary>
        public Dictionary<string, Progress> Progresses
        {
            get
            {
                if (_progresses == null)
                {
                    _progresses = new Dictionary<string, Progress>();
                }
                return _progresses;
            }
        }

        internal List<Watching> _watchings;
        /// <summary>
        /// 用户收视列表
        /// </summary>
        public List<Watching> Watchings
        {
            get
            {
                if (_watchings == null)
                {
                    _watchings = new List<Watching>();
                }
                return _watchings;
            }
        }

        internal List<BangumiTimeLine> _timeLine;
        /// <summary>
        /// 时间表
        /// </summary>
        public List<BangumiTimeLine> TimeLine
        {
            get
            {
                if (_timeLine == null)
                {
                    _timeLine = new List<BangumiTimeLine>();
                }
                return _timeLine;
            }
        }
    }
}
