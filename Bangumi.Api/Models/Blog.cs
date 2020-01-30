using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 日志
    /// </summary>
    public class Blog
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 概览
        /// </summary>
        [JsonProperty("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        [JsonProperty("image")]
        public string Image { get; set; }

        /// <summary>
        /// 回复数
        /// </summary>
        [JsonProperty("replies")]
        public int Replies { get; set; }

        /// <summary>
        /// 发布时间
        /// <br/>
        /// example: 1357144903
        /// </summary>
        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        /// <summary>
        /// 发布时间
        /// <br/>
        /// example: 2013-1-2 16:41
        /// </summary>
        [JsonProperty("dateline")]
        public string DateLine { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Blog b = (Blog)obj;
            return Id == b.Id &&
                   Timestamp == b.Timestamp &&
                   Replies == b.Replies &&
                   Url.EqualsExT(b.Url) &&
                   Title.EqualsExT(b.Title) &&
                   Summary.EqualsExT(b.Summary) &&
                   Image.EqualsExT(b.Image) &&
                   DateLine.EqualsExT(b.DateLine) &&
                   User.EqualsExT(b.User);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
