using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    public class Rating
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("count")]
        public RatingCount Count { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Rating r = (Rating)obj;
            return Total == r.Total &&
                   Score == r.Score &&
                   Count.EqualsExT(r.Count);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Total;
        }
    }
}
