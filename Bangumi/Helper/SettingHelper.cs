using Bangumi.Api.Utils;
using System;
using Windows.Storage;

namespace Bangumi.Helper
{
    internal static class SettingHelper
    {
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private static bool? _epsBatch;
        private static bool? _subjectComplete;
        private static bool? _useBangumiData;
        private static bool? _useBiliApp;
        private static bool? _useBangumiDataAirWeekday;
        private static long? _updateDay;

        static SettingHelper()
        {
            _epsBatch = localSettings.Values[nameof(EpsBatch)] as bool?;
            _subjectComplete = localSettings.Values[nameof(SubjectComplete)] as bool?;
            _useBangumiData = localSettings.Values[nameof(UseBangumiData)] as bool?;
            _useBiliApp = localSettings.Values[nameof(UseBiliApp)] as bool?;
            _useBangumiDataAirWeekday = localSettings.Values[nameof(UseBangumiDataAirWeekday)] as bool?;
            _updateDay = localSettings.Values["UpdateDay"] as long?;
        }

        public static bool EpsBatch
        {
            set
            {
                _epsBatch = value;
                localSettings.Values[nameof(EpsBatch)] = _epsBatch;
            }
            get
            {
                return _epsBatch == true;
            }
        }

        public static bool SubjectComplete
        {
            set
            {
                _subjectComplete = value;
                localSettings.Values[nameof(SubjectComplete)] = _subjectComplete;
            }
            get
            {
                return _subjectComplete == true;
            }
        }

        public static bool UseBangumiData
        {
            set
            {
                _useBangumiData = value;
                localSettings.Values[nameof(UseBangumiData)] = _useBangumiData;
            }
            get
            {
                return _useBangumiData == true;
            }
        }

        public static bool UseBiliApp
        {
            set
            {
                _useBiliApp = value;
                localSettings.Values[nameof(UseBiliApp)] = _useBiliApp;
            }
            get
            {
                return _useBiliApp == true;
            }
        }

        public static bool UseBangumiDataAirWeekday
        {
            set
            {
                _useBangumiDataAirWeekday = value;
                localSettings.Values[nameof(UseBangumiDataAirWeekday)] = _useBangumiDataAirWeekday;
            }
            get
            {
                return _useBangumiDataAirWeekday == true;
            }
        }

        public static bool IsUpdatedToday
        {
            set
            {
                if (value)
                {
                    _updateDay = DateTime.Today.ConvertDateTimeToJsTick();
                    localSettings.Values["UpdateDay"] = _updateDay;
                }
                else
                {
                    _updateDay = DateTime.Today.AddDays(-1).ConvertDateTimeToJsTick();
                    localSettings.Values["UpdateDay"] = _updateDay;
                }
            }
            get
            {
                return _updateDay == DateTime.Today.ConvertDateTimeToJsTick();
            }
        }

    }
}
