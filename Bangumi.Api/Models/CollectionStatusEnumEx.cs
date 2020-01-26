using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class CollectionStatusEnumEx
    {
        public static string GetDescCn(this CollectionStatusEnum status,
                                       SubjectTypeEnum subjectType)
        {
            switch (subjectType)
            {
                case SubjectTypeEnum.Book:
                    switch (status)
                    {
                        case CollectionStatusEnum.Wish:
                            return "想读";
                        case CollectionStatusEnum.Collect:
                            return "读过";
                        case CollectionStatusEnum.Do:
                            return "在读";
                        case CollectionStatusEnum.OnHold:
                            return "搁置";
                        case CollectionStatusEnum.Dropped:
                            return "抛弃";
                        default:
                            return "收藏";
                    }
                case SubjectTypeEnum.Music:
                    switch (status)
                    {
                        case CollectionStatusEnum.Wish:
                            return "想听";
                        case CollectionStatusEnum.Collect:
                            return "听过";
                        case CollectionStatusEnum.Do:
                            return "在听";
                        case CollectionStatusEnum.OnHold:
                            return "搁置";
                        case CollectionStatusEnum.Dropped:
                            return "抛弃";
                        default:
                            return "收藏";
                    }
                case SubjectTypeEnum.Game:
                    switch (status)
                    {
                        case CollectionStatusEnum.Wish:
                            return "想玩";
                        case CollectionStatusEnum.Collect:
                            return "玩过";
                        case CollectionStatusEnum.Do:
                            return "在玩";
                        case CollectionStatusEnum.OnHold:
                            return "搁置";
                        case CollectionStatusEnum.Dropped:
                            return "抛弃";
                        default:
                            return "收藏";
                    }
                case SubjectTypeEnum.Anime:
                case SubjectTypeEnum.Real:
                default:
                    switch (status)
                    {
                        case CollectionStatusEnum.Wish:
                            return "想看";
                        case CollectionStatusEnum.Collect:
                            return "看过";
                        case CollectionStatusEnum.Do:
                            return "在看";
                        case CollectionStatusEnum.OnHold:
                            return "搁置";
                        case CollectionStatusEnum.Dropped:
                            return "抛弃";
                        default:
                            return "收藏";
                    }
            }
        }

        public static string GetValue(this CollectionStatusEnum status)
        {
            switch (status)
            {
                case CollectionStatusEnum.Wish:
                    return "wish";
                case CollectionStatusEnum.Collect:
                    return "collect";
                case CollectionStatusEnum.Do:
                    return "do";
                case CollectionStatusEnum.OnHold:
                    return "on_hold";
                case CollectionStatusEnum.Dropped:
                    return "dropped";
                default:
                    return "收藏";
            }
        }

        public static CollectionStatusEnum? FromValue(string status)
        {
            switch (status)
            {
                case "wish":
                    return CollectionStatusEnum.Wish;
                case "collect":
                    return CollectionStatusEnum.Collect;
                case "do":
                    return CollectionStatusEnum.Do;
                case "on_hold":
                    return CollectionStatusEnum.OnHold;
                case "dropped":
                    return CollectionStatusEnum.Dropped;
                default:
                    return null;
            }
        }
    }
}
