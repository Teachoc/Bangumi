using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 某一条目用户收视进度
    /// </summary>
    public class Progress
    {
        [JsonProperty("subject_id")]
        public int SubjectId { get; set; }

        [JsonProperty("eps")]
        public List<EpStatus2> Eps { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Progress p = (Progress)obj;
            return Eps.SequenceEqualExT(p.Eps);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return SubjectId;
        }
    }
}
