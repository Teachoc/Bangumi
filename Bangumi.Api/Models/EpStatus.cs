using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 章节状态
    /// </summary>
    public class EpStatus
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("css_name")]
        public string CssName { get; set; }

        [JsonProperty("url_name")]
        public string UrlName { get; set; }

        [JsonProperty("cn_name")]
        public string CnName { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            EpStatus e = (EpStatus)obj;
            return Id == e.Id &&
                   CssName.EqualsExT(e.CssName) &&
                   UrlName.EqualsExT(e.UrlName) &&
                   CnName.EqualsExT(e.CnName);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
