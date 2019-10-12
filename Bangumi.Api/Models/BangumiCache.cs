using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class BangumiCache
    {
        /// <summary>
        /// 收藏，含动画、书籍、音乐、游戏、三次元
        /// </summary>
        internal Dictionary<string, Collection2> _collections;
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

        /// <summary>
        /// 条目详情
        /// </summary>
        internal Dictionary<string, Subject> _subjects;
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

        /// <summary>
        /// 条目收藏信息
        /// </summary>
        internal Dictionary<string, SubjectStatus2> _subjectStatus;
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

        /// <summary>
        /// 条目收视进度
        /// </summary>
        internal Dictionary<string, Progress> _progresses;
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

        /// <summary>
        /// 用户收视列表
        /// </summary>
        internal List<Watching> _watchings;
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

        /// <summary>
        /// 时间表
        /// </summary>
        internal List<BangumiTimeLine> _timeLine;
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
