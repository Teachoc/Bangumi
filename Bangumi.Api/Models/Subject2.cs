using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    public class Subject2
    {
        [JsonProperty("subject_id")]
        public int SubjectId { get; set; }

        [JsonProperty("subject")]
        public Subject Subject { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Subject2 s = (Subject2)obj;
            return SubjectId == s.SubjectId &&
                   Subject.EqualsExT(s.Subject);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return SubjectId;
        }
    }
}
