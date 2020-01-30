using Bangumi.Api.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    public class Calendar
    {
        [JsonProperty("weekday")]
        public Weekday Weekday { get; set; }

        [JsonProperty("items")]
        public List<SubjectForCalendar> Items { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Calendar b = (Calendar)obj;
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
