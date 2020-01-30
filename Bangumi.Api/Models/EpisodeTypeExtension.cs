using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class EpisodeTypeExtension
    {
        /// <summary>
        /// 获取章节类型的描述
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDesc(this EpisodeType type)
        {
            return type.ToString().Replace('_', '/');
        }
    }
}
