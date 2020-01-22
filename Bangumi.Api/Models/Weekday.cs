using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    public class Weekday
    {
        [JsonProperty("en")]
        public string English { get; set; }

        [JsonProperty("cn")]
        public string Chinese { get; set; }

        [JsonProperty("ja")]
        public string Japanese { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Weekday w = (Weekday)obj;
            return Id == w.Id &&
                   English.EqualsExT(w.English) &&
                   Chinese.EqualsExT(w.Chinese) &&
                   Japanese.EqualsExT(w.Japanese);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
