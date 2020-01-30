using Bangumi.Api.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 现实人物
    /// </summary>
    public class Person : Mono
    {
        /// <summary>
        /// 人物类型
        /// <br/>example: 主角
        /// </summary>
        [JsonProperty("role_name")]
        public string RoleName { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        [JsonProperty("jobs")]
        public List<string> Jobs { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            base.Equals(obj);

            Person s = (Person)obj;
            return RoleName.EqualsExT(s.RoleName) &&
                   Jobs.SequenceEqualExT(s.Jobs);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
