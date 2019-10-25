using Windows.Storage;

namespace Bangumi.Helper
{
    internal static class SettingHelper
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        private static bool? _epsBatch;
        private static bool? _subjectComplete;
        private static bool? _useBangumiData;
        private static bool? _useBiliApp;
        private static bool? _useBangumiDataAirSites;
        private static bool? _useBangumiDataAirTime;

        static SettingHelper()
        {
            _epsBatch = LocalSettings.Values[nameof(EpsBatch)] as bool?;
            _subjectComplete = LocalSettings.Values[nameof(SubjectComplete)] as bool?;
            _useBangumiData = LocalSettings.Values[nameof(UseBangumiData)] as bool?;
            _useBiliApp = LocalSettings.Values[nameof(UseBiliApp)] as bool?;
            _useBangumiDataAirSites = LocalSettings.Values[nameof(UseBangumiDataAirSites)] as bool?;
            _useBangumiDataAirTime = LocalSettings.Values[nameof(UseBangumiDataAirTime)] as bool?;
        }

        public static bool EpsBatch
        {
            set
            {
                _epsBatch = value;
                LocalSettings.Values[nameof(EpsBatch)] = _epsBatch;
            }
            get => _epsBatch == true;
        }

        public static bool SubjectComplete
        {
            set
            {
                _subjectComplete = value;
                LocalSettings.Values[nameof(SubjectComplete)] = _subjectComplete;
            }
            get => _subjectComplete == true;
        }

        public static bool UseBangumiData
        {
            set
            {
                _useBangumiData = value;
                LocalSettings.Values[nameof(UseBangumiData)] = _useBangumiData;
            }
            get => _useBangumiData == true;
        }

        public static bool UseBangumiDataAirSites
        {
            set
            {
                _useBangumiDataAirSites = value;
                LocalSettings.Values[nameof(UseBangumiDataAirSites)] = _useBangumiDataAirSites;
            }
            get => _useBangumiDataAirSites == true && UseBangumiData;
        }

        public static bool UseBiliApp
        {
            set
            {
                _useBiliApp = value;
                LocalSettings.Values[nameof(UseBiliApp)] = _useBiliApp;
            }
            get => _useBiliApp == true;
        }

        public static bool UseBangumiDataAirTime
        {
            set
            {
                _useBangumiDataAirTime = value;
                LocalSettings.Values[nameof(UseBangumiDataAirTime)] = _useBangumiDataAirTime;
            }
            get => _useBangumiDataAirTime == true && UseBangumiData;
        }

    }
}
