using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目详情
    /// </summary>
    public class Subject
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("eps_count")]
        public int EpsCount { get; set; }

        [JsonProperty("air_date")]
        public string AirDate { get; set; }

        [JsonProperty("air_weekday")]
        public int AirWeekday { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("rating")]
        public Rating Rating { get; set; }

        [JsonProperty("images")]
        public Images Images { get; set; }

        [JsonProperty("collection")]
        public CollectionStatus Collection { get; set; }

        internal List<Ep> _eps;
        [JsonProperty("eps")]
        public List<Ep> Eps
        {
            get
            {
                if (_eps == null)
                {
                    _eps = new List<Ep>();
                }
                return _eps;
            }
        }

        [JsonProperty("crt")]
        public List<Crt> Characters { get; set; }

        [JsonProperty("staff")]
        public List<Staff> Staff { get; set; }

        [JsonProperty("topic")]
        public List<Topic> Topics { get; set; }

        [JsonProperty("blog")]
        public List<Blog> Blogs { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Subject s = (Subject)obj;
            return Id == s.Id &&
                   Type == s.Type &&
                   Rank == s.Rank &&
                   EpsCount == s.EpsCount &&
                   AirWeekday == s.AirWeekday &&
                   Url.EqualsExT(s.Url) &&
                   Name.EqualsExT(s.Name) &&
                   NameCn.EqualsExT(s.NameCn) &&
                   Summary.EqualsExT(s.Summary) &&
                   AirDate.EqualsExT(s.AirDate) &&
                   Rating.EqualsExT(s.Rating) &&
                   Images.EqualsExT(s.Images) &&
                   Collection.EqualsExT(s.Collection) &&
                   Eps.SequenceEqualExT(s.Eps) &&
                   Characters.SequenceEqualExT(s.Characters) &&
                   Staff.SequenceEqualExT(s.Staff) &&
                   Topics.SequenceEqualExT(s.Topics) &&
                   Blogs.SequenceEqualExT(s.Blogs);

        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
