using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    public class Blog
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("replies")]
        public int Replies { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("dateline")]
        public string DateLine { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Blog b = (Blog)obj;
            return Id == b.Id &&
                   Timestamp == b.Timestamp &&
                   Replies == b.Replies &&
                   Url.EqualsExT(b.Url) &&
                   Title.EqualsExT(b.Title) &&
                   Summary.EqualsExT(b.Summary) &&
                   Image.EqualsExT(b.Image) &&
                   DateLine.EqualsExT(b.DateLine) &&
                   User.EqualsExT(b.User);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
