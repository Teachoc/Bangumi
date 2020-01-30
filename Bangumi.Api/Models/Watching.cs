using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 用户正在观看
    /// </summary>
    public class Watching
    {
        /// <summary>
        /// 条目名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 条目 ID
        /// </summary>
        [JsonProperty("subject_id")]
        public int SubjectId { get; set; }

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
        public int LastTouch { get; set; }

        [JsonProperty("subject")]
        public SubjectForWatching Subject { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Watching w = (Watching)obj;
            return SubjectId == w.SubjectId &&
                   EpStatus == w.EpStatus &&
                   VolStatus == w.VolStatus &&
                   LastTouch == w.LastTouch &&
                   Name.EqualsExT(w.Name) &&
                   Subject.EqualsExT(w.Subject);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return SubjectId;
        }
    }
}
