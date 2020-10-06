using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Bangumi.Data.Models
{

    public class Item
    {

        /// <summary>
        /// Examples: "新しい動画 3つのはなし", "ゲゲゲの鬼太郎", "巨人の星", "サスケ", "妖怪人間ベム"
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Examples: {"zh-Hans":["新动画 三个故事"],"en":["Three Tales"]}, {"zh-Hans":["鬼太郎","咯咯咯鬼太郎 1"]}, {"zh-Hans":["巨人之星"]}, {"zh-Hans":["鬼太郎"]}, {"zh-Hans":["死神少年","佐助"]}
        /// </summary>
        [JsonProperty("titleTranslate")]
        public TitleTranslate TitleTranslate { get; set; }

        /// <summary>
        /// Examples: "tv", "movie", "ova", "web"
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Examples: "ja", "en", "zh-Hans"
        /// </summary>
        [JsonProperty("lang")]
        public string Lang { get; set; }

        /// <summary>
        /// Examples: "", "http://www.tatsunoko.co.jp/works/archive/kurenai.html", "http://www.fujitv.co.jp/sazaesan/", "http://www.toei-anim.co.jp/lineup/tv/tigermask/", "http://www.tatsunoko.co.jp/tatsunoko_gekijo/"
        /// </summary>
        [JsonProperty("officialSite")]
        public string OfficialSite { get; set; }

        /// <summary>
        /// Examples: "1960-01-15T16:00:00Z", "1968-01-03T16:00:00Z", "1968-03-30T16:00:00Z", "1968-07-21T16:00:00Z", "1968-09-03T13:15:00Z"
        /// </summary>
        [JsonProperty("begin")]
        public DateTimeOffset? Begin { get; set; }

        /// <summary>
        /// Examples: "1960-01-15T16:30:00Z", "1969-03-30T16:30:00Z", "1971-09-18T16:30:00Z", "1968-07-21T17:00:00Z", "1969-03-25T13:45:00Z"
        /// </summary>
        [JsonProperty("end")]
        public DateTimeOffset? End { get; set; }

        /// <summary>
        /// Examples: [{"site":"bangumi","id":"213759"}], [{"site":"iqiyi","id":"a_19rrhb12tt","begin":"2015-10-29T08:31:41Z","official":true,"premuiumOnly":false,"censored":null,"exist":true,"comment":""},{"site":"bangumi","id":"40379"}], [{"site":"bangumi","id":"41983"},{"site":"nicovideo","id":"kyojin-no-hoshi","begin":"2012-12-24T06:00:00Z","official":true,"premuiumOnly":true,"censored":null,"exist":true,"comment":""}], [{"site":"bangumi","id":"211091"}], [{"site":"bangumi","id":"150325"}]
        /// </summary>
        [JsonProperty("sites")]
        public IList<Site> Sites { get; set; }

        /// <summary>
        /// Examples: <br/>
        /// 一次性："R/2020-01-01T13:00:00Z/P0D", <br/>
        /// 周播："R/2020-01-01T13:00:00Z/P7D", <br/>
        /// 日播："R/2020-01-01T13:00:00Z/P1D", <br/>
        /// 月播："R/2020-01-01T13:00:00Z/P1M"
        /// </summary>
        [JsonProperty("broadcast")]
        public string Broadcast { get; set; }

        /// <summary>
        /// Examples: ""
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; set; }
    }

}
