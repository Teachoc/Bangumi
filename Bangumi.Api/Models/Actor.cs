using Bangumi.Api.Common;
using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 人物
    /// </summary>
    public class Actor : MonoBase
    {
        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return base.Equals(obj);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
