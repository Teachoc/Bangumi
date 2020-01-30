using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 人物（基础模型）
    /// </summary>
    public class MonoBase
    {
        /// <summary>
        /// 人物 ID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 人物地址
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// large, medium, small, grid
        /// </summary>
        [JsonProperty("images")]
        public Images Images { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            MonoBase c = (MonoBase)obj;
            return Id == c.Id &&
                   Url.EqualsExT(c.Url) &&
                   Name.EqualsExT(c.Name) &&
                   Images.EqualsExT(c.Images);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
