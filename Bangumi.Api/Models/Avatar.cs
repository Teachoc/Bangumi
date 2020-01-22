using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    public class Avatar
    {
        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("small")]
        public string Small { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Avatar a = (Avatar)obj;
            return Large.EqualsExT(a.Large) &&
                   Medium.EqualsExT(a.Medium) &&
                   Small.EqualsExT(a.Small);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Medium.Length;
        }
    }
}
