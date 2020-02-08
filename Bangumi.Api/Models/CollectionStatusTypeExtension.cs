using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class CollectionStatusTypeExtension
    {
        public static string GetDesc(this CollectionStatusType status,
                                       SubjectType subjectType)
        {
            string ret = status switch
            {
                CollectionStatusType.Wish => "想做",
                CollectionStatusType.Collect => "做过",
                CollectionStatusType.Do => "在做",
                CollectionStatusType.OnHold => "搁置",
                CollectionStatusType.Dropped => "抛弃",
                _ => throw new NotImplementedException(),
            };
            return subjectType switch
            {
                SubjectType.Book => ret.Replace("做", "读"),
                SubjectType.Music => ret.Replace("做", "听"),
                SubjectType.Game => ret.Replace("做", "玩"),
                _ => ret.Replace("做", "看"),
            };
        }

        public static string GetValue(this CollectionStatusType status)
        {
            return status switch
            {
                CollectionStatusType.Wish => "wish",
                CollectionStatusType.Collect => "collect",
                CollectionStatusType.Do => "do",
                CollectionStatusType.OnHold => "on_hold",
                CollectionStatusType.Dropped => "dropped",
                _ => throw new NotImplementedException(),
            };
        }

        public static CollectionStatusType? FromValue(string status)
        {
            return status switch
            {
                "wish" => CollectionStatusType.Wish,
                "collect" => CollectionStatusType.Collect,
                "do" => CollectionStatusType.Do,
                "on_hold" => CollectionStatusType.OnHold,
                "dropped" => CollectionStatusType.Dropped,
                _ => null,
            };
        }
    }
}
