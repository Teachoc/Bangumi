using Bangumi.Api.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    public class Crt
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

        [JsonProperty("actors")]
        public List<Actor> Actors { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Crt c = (Crt)obj;
            return Id == c.Id &&
                   Url.EqualsExT(c.Url) &&
                   Name.EqualsExT(c.Name) &&
                   NameCn.EqualsExT(c.NameCn) &&
                   RoleName.EqualsExT(c.RoleName) &&
                   Images.EqualsExT(c.Images) &&
                   Actors.SequenceEqualExT(c.Actors);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
