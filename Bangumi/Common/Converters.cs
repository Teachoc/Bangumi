using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Bangumi.Api.Models;
using Bangumi.Api.Common;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace Bangumi.Common
{
    /// <summary>
    /// Provides static methods for use in x:Bind function binding to convert bound values to the required value.
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Returns the reverse of the provided value.
        /// </summary>
        public static bool Not(bool value) => !value;

        /// <summary>
        /// Returns true if the specified value is not null; otherwise, returns false.
        /// </summary>
        public static bool IsNotNull(object value) => value != null;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified value is true; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIf(bool value) =>
            value ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified value is null; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIfNull(object value) =>
            value == null ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified value is not null; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIfNotNull(object value) =>
            value == null ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Returns Visibility.Visible if the specified value is not null; otherwise, returns Visibility.Collapsed.
        /// </summary>
        public static Visibility CollapsedIfZero(int value) =>
            value == 0 ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified value is not null; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIfNotZero(int value) =>
            value == 0 ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified string is null or empty; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIfNullOrEmpty(string value) =>
            string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// 将 string 转换为 Brush
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Brush ConvertBrushFromString(string color)
        {
            System.Drawing.ColorConverter colorConverter = new System.Drawing.ColorConverter();
            try
            {
                System.Drawing.Color brushColor = (System.Drawing.Color)colorConverter.ConvertFromString(color);
                return new SolidColorBrush(Windows.UI.Color.FromArgb(brushColor.A, brushColor.R, brushColor.G, brushColor.B));
            }
            catch (Exception)
            {
                return new SolidColorBrush(Windows.UI.Colors.Gray);
            }
        }

        /// <summary>
        /// 将js时间戳转换为C#时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertJsTickToDateTime(long dateTime)
        {
            return dateTime.ToDateTime().ToString("yyyy-MM-dd HH:mm"); ;
        }

        /// <summary>
        /// 返回条目类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSubjectTypeName(SubjectTypeEnum type)
        {
            return type.GetDescCn();
        }

        /// <summary>
        /// 获取日期为星期几
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GetWeekday(string dateTime)
        {
            if (DateTime.TryParse(dateTime, out var d))
            {
                if (dateTime.Length > 10)
                {
                    return $"{System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(d.DayOfWeek)} {d.TimeOfDay:hh\\:mm}";
                }
                return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(d.DayOfWeek);
            }
            return string.Empty;
        }

        /// <summary>
        /// 根据收藏状态返回不同颜色
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static SolidColorBrush GetSolidColorBrush(CollectionStatusEnum? status)
        {
            switch (status)
            {
                case CollectionStatusEnum.Wish:
                    return (SolidColorBrush)Application.Current.Resources["WishBackground"];
                case CollectionStatusEnum.Collect:
                    return (SolidColorBrush)Application.Current.Resources["CollectBackground"];
                case CollectionStatusEnum.Do:
                    return (SolidColorBrush)Application.Current.Resources["DoBackground"];
                case CollectionStatusEnum.OnHold:
                    return (SolidColorBrush)Application.Current.Resources["OnHoldBackground"];
                case CollectionStatusEnum.Dropped:
                    return (SolidColorBrush)Application.Current.Resources["DroppedBackground"];
                default:
                    return (SolidColorBrush)Application.Current.Resources["DoBackground"];
            }
        }

        /// <summary>
        /// 根据收藏状态返回描述
        /// </summary>
        /// <param name="status"></param>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        public static string GetDesc(CollectionStatusEnum? status, SubjectTypeEnum subjectType = SubjectTypeEnum.Anime)
        {
            return status?.GetDescCn(subjectType) ?? string.Empty;
        }

        /// <summary>
        /// 获取星期的中文
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static string GetWeekday(int day)
        {
            switch (day)
            {
                case 1:
                    return "星期一";
                case 2:
                    return "星期二";
                case 3:
                    return "星期三";
                case 4:
                    return "星期四";
                case 5:
                    return "星期五";
                case 6:
                    return "星期六";
                case 7:
                    return "星期日";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 根据观看进度返回相应描述
        /// </summary>
        /// <param name="watchedEps"></param>
        /// <returns></returns>
        public static string GetWatchedEpsDesc(int watchedEps)
        {
            if (watchedEps == -1)
                return string.Empty;
            if (watchedEps == 0)
                return "尚未观看";
            return "看到" + watchedEps + "话";
        }

        /// <summary>
        /// 根据更新进度返回相应描述
        /// </summary>
        /// <param name="updatedEps"></param>
        /// <param name="eps"></param>
        /// <returns></returns>
        public static string GetUpdatedEpsDesc(int updatedEps, List<ViewModels.SimpleEp> eps)
        {
            if (updatedEps == -1)
                return "无章节";
            if (updatedEps == 0)
                return "尚未放送";
            if (eps != null)
            {
                return eps.Count(ep => ep.Type == 0) == updatedEps ? "全" + updatedEps + "话" : "更新到" + updatedEps + "话";
            }
            return "共" + updatedEps + "话";
        }

        /// <summary>
        /// 根据sort和type返回章节相应描述
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetEpSortDesc(double sort, int type)
        {
            if (type == 0)
            {
                return "第 " + sort + " 话";
            }
            else
            {
                return type.GetEpisodeType() + " " + sort;
            }
        }

        /// <summary>
        /// 根据next_ep和eps返回非正片章节相应描述
        /// </summary>
        /// <param name="nextEp"></param>
        /// <param name="eps"></param>
        /// <returns></returns>
        public static string GetEpNextSortDesc(double nextEp, List<ViewModels.SimpleEp> eps)
        {
            if (nextEp == -1)
            {
                return "EP.";
            }
            var ep = eps?.Where(p => p.Sort == nextEp && Regex.IsMatch(p.Status, "(Air|Today|NA)")).FirstOrDefault();
            if (ep == null)
            {
                return "EP.";
            }
            else if (ep.Type == 0)
            {
                return "EP." +
                       nextEp.ToString() +
                       (string.IsNullOrEmpty(ep.Name) ? string.Empty : " " + ep.Name);
            }
            else
            {
                return "EP." +
                       ep.Type.GetEpisodeType() + " " + nextEp +
                       (string.IsNullOrEmpty(ep.Name) ? string.Empty : " " + ep.Name);
            }
        }

        /// <summary>
        /// 返回 a{splitter}b 的形式字符串
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="splitter">分隔符</param>
        /// <returns></returns>
        public static string Split(object a, object b, string splitter)
        {
            return $"{a}{splitter}{b}";
        }

        /// <summary>
        /// 保留小数位数
        /// </summary>
        /// <param name="d"></param>
        /// <param name="n">小数点后几位</param>
        /// <returns></returns>
        public static string DoubleToString(double d, int n)
        {
            if (d == 0)
                return "-";
            else
                return Math.Round(d, n, MidpointRounding.AwayFromZero).ToString();
        }

        /// <summary>
        /// 根据评分返回对应的描述
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static string GetRateDesc(double rate)
        {
            string[] descs = { string.Empty, "不忍直视", "很差", "差", "较差", "不过不失", "还行", "推荐", "力荐", "神作", "超神作 (请谨慎评价)" };
            return descs[(int)Math.Round(rate, 0, MidpointRounding.AwayFromZero)];
        }

        /// <summary>
        /// 将演员列表转为string
        /// </summary>
        /// <param name="actors"></param>
        /// <returns></returns>
        public static string ActorListToString(List<Actor> actors)
        {
            if (actors != null && actors.Count != 0)
            {
                return "CV：" + string.Join('、', actors.Select(a => a.Name));
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// 将职责列表转为string
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public static string JobListToString(List<string> jobs)
        {
            if (jobs != null && jobs.Count != 0)
            {
                return string.Join('、', jobs);
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// 拼接字符串
        /// </summary>
        /// <param name="vs"></param>
        /// <returns></returns>
        public static string ConcatStrings(IEnumerable<string> vs)
        {
            return string.Concat(vs);
        }
        public static string ConcatStrings(string str0, string str1, string str2, string str3)
        {
            return string.Concat(str0, str1, str2, str3);
        }

    }
}
