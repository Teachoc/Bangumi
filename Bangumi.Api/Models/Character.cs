using Bangumi.Api.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 虚拟角色
    /// </summary>
    public class Character : Mono
    {
        [JsonProperty("actors")]
        public List<Actor> Actors { get; set; }

        /// <summary>
        /// 角色类型
        /// <br/>example: 主角
        /// </summary>
        [JsonProperty("role_name")]
        public string RoleName { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            base.Equals(obj);

            Character c = (Character)obj;
            return RoleName.EqualsExT(c.RoleName) &&
                   Actors.SequenceEqualExT(c.Actors);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
