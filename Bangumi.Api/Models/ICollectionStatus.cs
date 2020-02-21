using System.ComponentModel;

namespace Bangumi.Api.Models
{
    public interface ICollectionStatus : INotifyPropertyChanged
    {
        /// <summary>
        /// 用来保存当前用户的收藏状态
        /// </summary>
        CollectionStatusType? Status { get; set; }
    }
}
