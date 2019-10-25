using Newtonsoft.Json;

namespace Bangumi.Api.Models
{
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("nickname")]
        public string NickName { get; set; }

        [JsonProperty("avatar")]
        public Avatar Avatar { get; set; }

        [JsonProperty("sign")]
        public string Sign { get; set; }

        [JsonProperty("usergroup")]
        public int UserGroup { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            User u = (User)obj;
            return Id == u.Id &&
                   UserGroup == u.UserGroup &&
                   Url.EqualsExT(u.Url) &&
                   UserName.EqualsExT(u.UserName) &&
                   NickName.EqualsExT(u.NickName) &&
                   Sign.EqualsExT(u.Sign) &&
                   Avatar.EqualsExT(u.Avatar);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
