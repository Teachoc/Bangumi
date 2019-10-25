using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 某一类别的收藏
    /// </summary>
    public class Collection2
    {
        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("collects")]
        public List<Collection> Collects { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Collection2 c = (Collection2)obj;
            return Type == c.Type &&
                   Name.EqualsExT(c.Name) &&
                   NameCn.EqualsExT(c.NameCn) &&
                   Collects.SequenceEqualExT(c.Collects);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Type;
        }
    }
}
