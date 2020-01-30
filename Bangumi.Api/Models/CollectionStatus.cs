using Bangumi.Api.Common;
using Newtonsoft.Json;
using System;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 收藏状态
    /// <br/> 1 = wish = 想做
    /// <br/> 2 = collect = 做过
    /// <br/> 3 = do = 在做
    /// <br/> 4 = on_hold = 搁置
    /// <br/> 5 = dropped = 抛弃
    /// </summary>
    public class CollectionStatus
    {
        /// <summary>
        /// 收藏状态 ID
        /// </summary>
        [JsonProperty("id")]
        public CollectionStatusType Id { get; set; }

        /// <summary>
        /// 收藏状态类型
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// 收藏状态名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            CollectionStatus s = (CollectionStatus)obj;
            return Id == s.Id &&
                   Type.EqualsExT(s.Type) &&
                   Name.EqualsExT(s.Name);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (int)Id;
        }
    }
}
