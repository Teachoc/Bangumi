using Bangumi.Api.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目的收藏信息
    /// </summary>
    public class CollectionStatusE
    {
        [JsonProperty("status")]
        public CollectionStatus Status { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        [JsonProperty("rating")]
        public int Rating { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [JsonProperty("tag")]
        public List<string> Tags { get; set; }

        /// <summary>
        /// 章节观看状态
        /// </summary>
        [JsonProperty("ep_status")]
        public int EpStatus { get; set; }

        /// <summary>
        /// 书籍章节状态
        /// </summary>
        [JsonProperty("vol_status")]
        public int VolStatus { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [JsonProperty("lasttouch")]
        public long LastTouch { get; set; }

        /// <summary>
        /// 仅自己可见
        /// </summary>
        [JsonProperty("private")]
        public string Private { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            CollectionStatusE s = (CollectionStatusE)obj;
            return Rating == s.Rating &&
                   LastTouch == s.LastTouch &&
                   EpStatus == s.EpStatus &&
                   VolStatus == s.VolStatus &&
                   Comment.EqualsExT(s.Comment) &&
                   Private.EqualsExT(s.Private) &&
                   Status.EqualsExT(s.Status) &&
                   User.EqualsExT(s.User) &&
                   Tags.SequenceEqualExT(s.Tags);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (int)LastTouch;
        }
    }
}
