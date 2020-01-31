using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 人物
    /// </summary>
    public class Mono : MonoBase
    {
        /// <summary>
        /// 简体中文名
        /// </summary>
        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        /// <summary>
        /// 回复数量
        /// </summary>
        [JsonProperty("comment")]
        public int Comment { get; set; }

        /// <summary>
        /// 收藏人数
        /// </summary>
        [JsonProperty("collects")]
        public int Collects { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Mono c = (Mono)obj;
            return base.Equals(obj) &&
                   NameCn.EqualsExT(c.NameCn) &&
                   Comment == c.Comment &&
                   Collects == c.Collects;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
