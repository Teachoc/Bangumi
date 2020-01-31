using Bangumi.Api.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class SubjectForCalendar : SubjectBase, ICollectionStatus
    {
        public CollectionStatusType? Status { get; set; }

        /// <summary>
        /// 排名
        /// </summary>
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("rating")]
        public Rating Rating { get; set; }

        /// <summary>
        /// doing
        /// </summary>
        [JsonProperty("collection")]
        public SubjectCollection Collection { get; set; }


        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectForCalendar s = (SubjectForCalendar)obj;
            return base.Equals(obj) &&
                   Rank == s.Rank &&
                   Rating.EqualsExT(s.Rating) &&
                   Collection.EqualsExT(s.Collection) &&
                   Status == s.Status;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
