using Bangumi.Api.Common;
using Newtonsoft.Json;
using System;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目状态
    /// </summary>
    public class SubjectStatus
    {
        [JsonProperty("id")]
        public CollectionStatusEnum Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectStatus s = (SubjectStatus)obj;
            return Id == s.Id &&
                   Type.EqualsExT(s.Type);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (int)Id;
        }
    }
}
