﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
        public SubjectStatus2 Collection { get; set; }

        [JsonProperty("eps")]
        public List<Ep> Eps { get; set; }

        [JsonProperty("crt")]
        public List<Crt> Characters { get; set; }

        [JsonProperty("staff")]
        public List<Staff> Staff { get; set; }

        [JsonProperty("topic")]
        public List<Topic> Topics { get; set; }

        [JsonProperty("blog")]
        public List<Blog> Blogs { get; set; }
    }
}