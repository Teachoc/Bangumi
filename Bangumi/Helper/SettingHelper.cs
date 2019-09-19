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
        private static long? _updateDay;

        static SettingHelper()
        {
            _epsBatch = localSettings.Values["EpsBatch"] as bool?;
            _subjectComplete = localSettings.Values["SubjectComplete"] as bool?;
            _useBangumiData = localSettings.Values["UseBangumiData"] as bool?;
            _useBiliApp = localSettings.Values["UseBiliApp"] as bool?;
            _updateDay = localSettings.Values["UpdateDay"] as long?;
        }

        public static bool EpsBatch
        {
            set
            {
                _epsBatch = value;
                localSettings.Values["EpsBatch"] = _epsBatch;
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
                localSettings.Values["SubjectComplete"] = _subjectComplete;
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
                localSettings.Values["UseBangumiData"] = _useBangumiData;
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
                localSettings.Values["UseBiliApp"] = _useBiliApp;
            }
            get
            {
                return _useBiliApp == true;
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
