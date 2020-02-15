using Bangumi.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bangumi.Api.Common
{
    public static class Extensions
    {
        /// <summary>
        /// 将C#时间戳转换为js时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int ToJsTick(this DateTime dateTime)
        {
            // C# 时间戳为 1/10000000 秒，从0001年1月1日开始；Bangumi 使用的时间戳为秒，从1970年1月1日开始
            // 默认 DateTime 使用 UTC 时间
            return (int)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// 将js时间戳转换为C#时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this int dateTime)
        {
            // C# 时间戳为 1/10000000 秒，从0001年1月1日开始；Bangumi 使用的时间戳为秒，从1970年1月1日开始
            // 默认 DateTime 使用 UTC 时间
            return new DateTime(1970, 1, 1).Add(new TimeSpan(long.Parse(dateTime + "0000000"))).ToLocalTime();
        }

        /// <summary>
        /// 若网址是https的则直接返回，否则将http替换为https后返回
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static string ToHttps(this string http)
        {
            if (http == null)
            {
                throw new ArgumentNullException(nameof(http));
            }
            if (http.StartsWith("http"))
            {
                return http.StartsWith("https") ? http : http.Insert(4, "s");
            }
            return http;
        }

        /// <summary>
        /// 将Images中的图片链接替换为https
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static void ConvertImageHttpToHttps(this Images image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            image.Grid = image.Grid?.ToHttps();
            image.Small = image.Small?.ToHttps();
            image.Common = image.Common?.ToHttps();
            image.Medium = image.Medium?.ToHttps();
            image.Large = image.Large?.ToHttps();
        }

        /// <summary>
        /// 对象比较，支持 null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool EqualsExT<T>(this T o, T obj) where T : class
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

        /// <summary>
        /// 文件名转换为小写，
        /// 与文件夹组合为路径，
        /// 将 '_' 替换为 '.'
        /// </summary>
        /// <param name="file"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string GetFilePath(this AppFile file, string folder)
        {
            return Path.Combine(folder, file.ToString().ToLowerInvariant().Replace('_', '.'));
        }

    }
}
