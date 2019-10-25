using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 收藏的条目的状态信息
    /// </summary>
    public class SubjectStatus2
    {
        [JsonProperty("status")]
        public SubjectStatus Status { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("tag")]
        public List<string> Tags { get; set; }

        [JsonProperty("ep_status")]
        public int EpStatus { get; set; }

        [JsonProperty("lasttouch")]
        public long LastTouch { get; set; }

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

            SubjectStatus2 s = (SubjectStatus2)obj;
            return Rating == s.Rating &&
                   EpStatus == s.EpStatus &&
                   LastTouch == s.LastTouch &&
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
