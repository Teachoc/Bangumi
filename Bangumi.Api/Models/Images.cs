using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    public class Images
    {
        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("common")]
        public string Common { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("grid")]
        public string Grid { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Images i = (Images)obj;
            return Large.EqualsExT(i.Large) &&
                   Common.EqualsExT(i.Common) &&
                   Medium.EqualsExT(i.Medium) &&
                   Small.EqualsExT(i.Small) &&
                   Grid.EqualsExT(i.Grid);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Common.Length;
        }
    }
}
