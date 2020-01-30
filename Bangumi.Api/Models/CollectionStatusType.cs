namespace Bangumi.Api.Models
{
    /// <summary>
    /// 收藏状态类型
    /// [ wish, collect, do, on_hold, dropped ]
    /// </summary>
    public enum CollectionStatusType
    {
        Wish = 1,
        Collect = 2,
        Do = 3,
        OnHold = 4,
        Dropped = 5
    }
}
