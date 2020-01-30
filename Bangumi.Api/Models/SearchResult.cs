using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class SearchResult
    {
        [JsonProperty("results")]
        public int ResultCount { get; set; }

        [JsonProperty("list")]
        public List<SubjectForSearch> Results { get; set; }
    }
}
