using Newtonsoft.Json;
using System;

namespace Bangumi.Data.Models
{
    internal class VersionInfo
    {
        public string Version { get; set; }
        public bool AutoCheck { get; set; } = false;
        public bool AutoUpdate { get; set; } = false;
        public DateTimeOffset LastUpdate { get; set; }
        public int CheckInterval { get; set; } = 7;
        public string[] SitesEnabledOrder { get; set; }

        public string ToJson() =>
            JsonConvert.SerializeObject(this);

        public static VersionInfo FromJson(string json) =>
            JsonConvert.DeserializeObject<VersionInfo>(json);
    }
}
