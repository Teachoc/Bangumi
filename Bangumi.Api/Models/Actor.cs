﻿using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 人物
    /// </summary>
    public class Actor
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 人物主页
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 人物姓名
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

            Actor a = (Actor)obj;
            return Id == a.Id &&
                   Url.EqualsExT(a.Url) &&
                   Name.EqualsExT(a.Name) &&
                   Images.EqualsExT(a.Images);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
