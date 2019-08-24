﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Staff
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("role_name")]
        public string RoleName { get; set; }

        [JsonProperty("images")]
        public Images Images { get; set; }

        [JsonProperty("jobs")]
        public List<string> Jobs { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Staff s = (Staff)obj;
            return Id == s.Id &&
                   (Url == null ? Url == s.Url : Url.Equals(s.Url)) &&
                   (Name == null ? Name == s.Name : Name.Equals(s.Name)) &&
                   (NameCn == null ? NameCn == s.NameCn : NameCn.Equals(s.NameCn)) &&
                   (RoleName == null ? RoleName == s.RoleName : RoleName.Equals(s.RoleName)) &&
                   (Images == null ? Images == s.Images : Images.Equals(s.Images)) &&
                   (Jobs == null ? Jobs == s.Jobs : Jobs.SequenceEqual(s.Jobs));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
