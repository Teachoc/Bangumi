using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 章节状态2
    /// </summary>
    public class EpStatusE
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public EpStatus Status { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            EpStatusE e = (EpStatusE)obj;
            return Id == e.Id &&
                   Status.EqualsExT(e.Status);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
