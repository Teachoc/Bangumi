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

        public static Visibility CollapsedIfAllNullOrEmpty(string value, string value2, string value3) =>
            (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(value2) && string.IsNullOrEmpty(value3)) ?
            Visibility.Collapsed :
            Visibility.Visible;

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
        public static string ConvertJsTickToDateTime(int dateTime)
        {
            return dateTime.ToDateTime().ToString("yyyy-MM-dd HH:mm");
        }

        /// <summary>
        /// 返回条目类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSubjectTypeName(SubjectType type)
        {
            return type.GetDesc();
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
        public static SolidColorBrush GetSolidColorBrush(CollectionStatusType? status)
        {
            return status switch
            {
                CollectionStatusType.Wish => (SolidColorBrush)Application.Current.Resources["WishBackground"],
                CollectionStatusType.Collect => (SolidColorBrush)Application.Current.Resources["CollectBackground"],
                CollectionStatusType.Do => (SolidColorBrush)Application.Current.Resources["DoBackground"],
                CollectionStatusType.OnHold => (SolidColorBrush)Application.Current.Resources["OnHoldBackground"],
                CollectionStatusType.Dropped => (SolidColorBrush)Application.Current.Resources["DroppedBackground"],
                _ => (SolidColorBrush)Application.Current.Resources["DoBackground"],
            };
        }

        /// <summary>
        /// 根据收藏状态返回描述
        /// </summary>
        /// <param name="status"></param>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        public static string GetDesc(CollectionStatusType? status, SubjectType subjectType = SubjectType.Anime)
        {
            return status?.GetDesc(subjectType) ?? string.Empty;
        }

        /// <summary>
        /// 根据章节放送状态和章节状态类型返回对应颜色
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SolidColorBrush GetEpBackground(string status, EpStatusType type)
        {
            return type switch
            {
                EpStatusType.watched => (SolidColorBrush)Application.Current.Resources["EpWatchedBackground"],
                EpStatusType.queue => (SolidColorBrush)Application.Current.Resources["EpQueueBackground"],
                EpStatusType.drop => (SolidColorBrush)Application.Current.Resources["EpDropBackground"],
                _ => status switch
                {
                    "Air" => (SolidColorBrush)Application.Current.Resources["EpAirBackground"],
                    "Today" => (SolidColorBrush)Application.Current.Resources["EpTodayBackground"],
                    "NA" => (SolidColorBrush)Application.Current.Resources["EpNABackground"],
                    _ => (SolidColorBrush)Application.Current.Resources["EpBackground"]
                },
            };
        }
        public static SolidColorBrush GetEpForeground(string status, EpStatusType type)
        {
            return type switch
            {
                EpStatusType.watched => (SolidColorBrush)Application.Current.Resources["EpWatchedForeground"],
                EpStatusType.queue => (SolidColorBrush)Application.Current.Resources["EpQueueForeground"],
                EpStatusType.drop => (SolidColorBrush)Application.Current.Resources["EpDropForeground"],
                _ => status switch
                {
                    "Air" => (SolidColorBrush)Application.Current.Resources["EpAirForeground"],
                    "Today" => (SolidColorBrush)Application.Current.Resources["EpTodayForeground"],
                    "NA" => (SolidColorBrush)Application.Current.Resources["EpNAForeground"],
                    _ => (SolidColorBrush)Application.Current.Resources["EpForeground"]
                },
            };
        }
        public static SolidColorBrush GetEpBorder(string status, EpStatusType type)
        {
            return type switch
            {
                EpStatusType.watched => (SolidColorBrush)Application.Current.Resources["EpWatchedBorder"],
                EpStatusType.queue => (SolidColorBrush)Application.Current.Resources["EpQueueBorder"],
                EpStatusType.drop => (SolidColorBrush)Application.Current.Resources["EpDropBorder"],
                _ => status switch
                {
                    "Air" => (SolidColorBrush)Application.Current.Resources["EpAirBorder"],
                    "Today" => (SolidColorBrush)Application.Current.Resources["EpTodayBorder"],
                    "NA" => (SolidColorBrush)Application.Current.Resources["EpNABorder"],
                    _ => (SolidColorBrush)Application.Current.Resources["EpBorder"]
                },
            };
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
            {
                return "-";
            }
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
