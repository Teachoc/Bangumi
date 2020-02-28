using System;

namespace Bangumi.Api.Models
{
    public static class SubjectTypeExtension
    {
        public static string GetDesc(this SubjectType type)
        {
            return type switch
            {
                SubjectType.Book => "书籍",
                SubjectType.Anime => "动画",
                SubjectType.Music => "音乐",
                SubjectType.Game => "游戏",
                SubjectType.Real => "三次元",
                _ => type.ToString(),
            };
        }

        /// <summary>
        /// 条目类型名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetValue(this SubjectType type)
        {
            return type switch
            {
                SubjectType.Book => "book",
                SubjectType.Anime => "anime",
                SubjectType.Music => "music",
                SubjectType.Game => "game",
                SubjectType.Real => "real",
                _ => type.ToString().ToLower(),
            };
        }

        /// <summary>
        /// 条目类型枚举
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SubjectType ToSubjectType(this string type)
        {
            return type switch
            {
                "book" => SubjectType.Book,
                "anime" => SubjectType.Anime,
                "music" => SubjectType.Music,
                "game" => SubjectType.Game,
                "real" => SubjectType.Real,
                _ => throw new Exception("No such enum.")
            };
        }
    }
}
