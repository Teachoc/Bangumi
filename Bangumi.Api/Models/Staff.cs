using Bangumi.Api.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    public class Staff
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("role_name")]
        public string RoleName { get; set; }

        [JsonProperty("images")]
        public Images Images { get; set; }

        [JsonProperty("jobs")]
        public List<string> Jobs { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Staff s = (Staff)obj;
            return Id == s.Id &&
                   Url.EqualsExT(s.Url) &&
                   Name.EqualsExT(s.Name) &&
                   NameCn.EqualsExT(s.NameCn) &&
                   RoleName.EqualsExT(s.RoleName) &&
                   Images.EqualsExT(s.Images) &&
                   Jobs.SequenceEqualExT(s.Jobs);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
