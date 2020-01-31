using Bangumi.Api.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class SubjectForSearch : SubjectBase, ICollectionStatus
    {
        public CollectionStatusType? Status { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectForSearch s = (SubjectForSearch)obj;
            return base.Equals(obj) &&
                   Status == s.Status;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
