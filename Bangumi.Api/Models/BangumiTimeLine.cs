using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    public class BangumiTimeLine
    {
        [JsonProperty("weekday")]
        public Weekday Weekday { get; set; }

        [JsonProperty("items")]
        public List<Subject> Items { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            BangumiTimeLine b = (BangumiTimeLine)obj;
            return Weekday.EqualsExT(b.Weekday) &&
                   Items.SequenceEqualExT(b.Items);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Items.Count;
        }
    }
}
