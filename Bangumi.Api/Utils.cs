using Bangumi.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bangumi.Api
{
    public static class Utils
    {
        /// <summary>
        /// 将C#时间戳转换为js时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ConvertDateTimeToJsTick(this DateTime dateTime)
        {
            // C# 时间戳为 1/10000000 秒，从0001年1月1日开始；Bangumi 使用的时间戳为秒，从1970年1月1日开始
            // 默认 DateTime 使用 UTC 时间
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// 若网址是https的则直接返回，否则将http替换为https后返回
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static string ConvertHttpToHttps(this string http)
        {
            if (http.StartsWith("http"))
                return http.StartsWith("https") ? http : http.Insert(4, "s");
            else
                return http;
        }

        /// <summary>
        /// 将Images中的图片链接替换为https
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static void ConvertImageHttpToHttps(this Images image)
        {
            image.Grid = image.Grid.ConvertHttpToHttps();
            image.Small = image.Small.ConvertHttpToHttps();
            image.Common = image.Common.ConvertHttpToHttps();
            image.Medium = image.Medium.ConvertHttpToHttps();
            image.Large = image.Large.ConvertHttpToHttps();
        }

        /// <summary>
        /// 对象比较，支持 null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool EqualsExT<T>(this T o, T obj)
        {
            return o == null ? obj == null : o.Equals(obj);
        }

        /// <summary>
        /// List 比较，支持 null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool SequenceEqualExT<T>(this IEnumerable<T> o, IEnumerable<T> obj)
        {
            return (o == null ? obj == null : (o.Count() == 0 ? (obj != null && obj.Count() == 0) : o.SequenceEqual(obj)));
        }
    }
}
