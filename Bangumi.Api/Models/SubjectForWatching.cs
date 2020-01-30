using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 简略条目详情，适用于Watching
    /// </summary>
    public class SubjectForWatching : SubjectBase
    {
        /// <summary>
        /// 话数
        /// </summary>
        [JsonProperty("eps")]
        public int Eps { get; set; }

        /// <summary>
        /// 话数
        /// </summary>
        [JsonProperty("eps_count")]
        public int EpsCount { get; set; }

        /// <summary>
        /// doing
        /// </summary>
        [JsonProperty("collection")]
        public SubjectCollection Collection { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            base.Equals(obj);

            SubjectForWatching s = (SubjectForWatching)obj;
            return Eps.EqualsExT(s.Eps) &&
                   EpsCount == s.EpsCount &&
                   Collection.EqualsExT(s.Collection);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
