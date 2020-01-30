using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class EpStatusTypeExtension
    {
        public static string GetCnName(this EpStatusType status)
        {
            return status switch
            {
                EpStatusType.watched => "看过",
                EpStatusType.queue => "想看",
                EpStatusType.drop => "抛弃",
                _ => "",
            };
        }

        public static string GetCssName(this EpStatusType status)
        {
            return status switch
            {
                EpStatusType.watched => "Watched",
                EpStatusType.queue => "Queue",
                EpStatusType.drop => "Drop",
                _ => "",
            };
        }

        public static string GetUrlName(this EpStatusType status)
        {
            return status switch
            {
                EpStatusType.watched => "watched",
                EpStatusType.queue => "queue",
                EpStatusType.drop => "drop",
                _ => "",
            };
        }
    }
}
