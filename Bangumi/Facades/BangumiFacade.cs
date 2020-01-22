using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Helper;
using System;
using System.Threading.Tasks;

namespace Bangumi.Facades
{
    public static class BangumiFacade
    {
        #region 更新进度、状态，并显示通知

        /// <summary>
        /// 更新收视进度。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <returns>更新是否成功</returns>
        public static async Task<bool> UpdateProgressAsync(string ep, EpStatusEnum status)
        {
            try
            {
                if (await BangumiApi.BgmApi.UpdateProgress(ep, status))
                {
                    NotificationHelper.Notify($"标记章节{ep}{status.GetCnName()}成功");
                    return true;
                }
                NotificationHelper.Notify($"标记章节{ep}{status.GetCnName()}失败，请重试！",
                                          NotificationHelper.NotifyType.Warn);
                return false;
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("更新收视进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
                return false;
            }
        }

        /// <summary>
        /// 批量更新收视进度。
        /// 使用 HttpWebRequest 提交表单进行更新，更新收藏状态使用相同方法。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <param name="epsId"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressBatchAsync(int ep, EpStatusEnum status, string epsId)
        {
            try
            {
                if (await BangumiApi.BgmApi.UpdateProgressBatch(ep, status, epsId))
                {
                    NotificationHelper.Notify($"批量标记章节{epsId}{status.GetCnName()}状态成功");
                    return true;
                }
                NotificationHelper.Notify($"批量标记章节{epsId}{status.GetCnName()}状态失败，请重试！",
                                          NotificationHelper.NotifyType.Warn);
                return false;
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("批量标记章节状态失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
                return false;
            }
        }

        /// <summary>
        /// 更新收藏状态
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="collectionStatus"></param>
        /// <param name="comment"></param>
        /// <param name="rating"></param>
        /// <param name="privace"></param>
        /// <returns>更新是否成功</returns>
        public static async Task<bool> UpdateCollectionStatusAsync(string subjectId,
            CollectionStatusEnum collectionStatus, string comment = "", string rating = "", string privace = "0")
        {
            try
            {
                if (await BangumiApi.BgmApi.UpdateStatus(subjectId, collectionStatus, comment, rating, privace)
                    .ContinueWith(t => t.Result?.Status.Type == collectionStatus.GetValue()))
                {
                    NotificationHelper.Notify($"更新条目{subjectId}状态成功");
                    return true;
                }
                NotificationHelper.Notify($"更新条目{subjectId}状态失败，请重试！",
                                          NotificationHelper.NotifyType.Warn);
                return false;
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("更新条目状态失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
                return false;
            }
        }

        #endregion



    }

}
