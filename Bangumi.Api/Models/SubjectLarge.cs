using Bangumi.Api.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目详情
    /// </summary>
    public class SubjectLarge : SubjectBase
    {
        /// <summary>
        /// 话数
        /// </summary>
        [JsonProperty("eps_count")]
        public int EpsCount { get; set; }

        /// <summary>
        /// 排名
        /// </summary>
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("rating")]
        public Rating Rating { get; set; }

        [JsonProperty("collection")]
        public SubjectCollection Collection { get; set; }

        /// <summary>
        /// 章节
        /// </summary>
        [JsonProperty("eps")]
        public List<Episode> Eps { get; set; }

        /// <summary>
        /// 角色信息
        /// </summary>
        [JsonProperty("crt")]
        public List<Character> Characters { get; set; }

        /// <summary>
        /// 制作人员信息
        /// </summary>
        [JsonProperty("staff")]
        public List<Person> Staff { get; set; }

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

            SubjectLarge s = (SubjectLarge)obj;
            return base.Equals(obj) &&
                   Rank == s.Rank &&
                   EpsCount == s.EpsCount &&
                   Rating.EqualsExT(s.Rating) &&
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
