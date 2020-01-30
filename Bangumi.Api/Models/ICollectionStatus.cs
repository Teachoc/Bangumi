using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public interface ICollectionStatus
    {
        /// <summary>
        /// 用来保存当前用户的收藏状态
        /// </summary>
        CollectionStatusType? Status { get; set; }
    }
}
