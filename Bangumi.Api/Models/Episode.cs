using Bangumi.Api.Common;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bangumi.Api.Models
{
    public class Episode
    {
        /// <summary>
        /// 章节 ID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 章节地址
        /// <br/>http://bgm.tv/ep/1027
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 章节类型
        /// </summary>
        [JsonProperty("type")]
        public EpisodeType Type { get; set; }

        /// <summary>
        /// 集数
        /// </summary>
        [JsonProperty("sort")]
        public double Sort { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 简体中文标题
        /// </summary>
        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        /// <summary>
        /// 时长
        /// <br/>24m
        /// </summary>
        [JsonProperty("duration")]
        public string Duration { get; set; }

        /// <summary>
        /// 放送日期
        /// <br/>2002-04-03
        /// </summary>
        [JsonProperty("airdate")]
        public string AirDate { get; set; }

        /// <summary>
        /// 回复数量
        /// </summary>
        [JsonProperty("comment")]
        public int Comment { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [JsonProperty("desc")]
        public string Desc { get; set; }

        /// <summary>
        /// 放送状态
        /// <br/>Air
        /// <br/>Today
        /// <br/>NA
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }


        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Episode e = (Episode)obj;
            return Id == e.Id &&
                   Type == e.Type &&
                   Sort == e.Sort &&
                   Comment == e.Comment &&
                   Url.EqualsExT(e.Url) &&
                   Name.EqualsExT(e.Name) &&
                   NameCn.EqualsExT(e.NameCn) &&
                   Duration.EqualsExT(e.Duration) &&
                   AirDate.EqualsExT(e.AirDate) &&
                   Desc.EqualsExT(e.Desc) &&
                   Status.EqualsExT(e.Status);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
