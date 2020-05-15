using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Bangumi.Helper
{
    internal static class SettingHelper
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        private static bool Get([CallerMemberName]string key = null)
        {
            return LocalSettings.Values[key] as bool? == true;
        }

        private static void Set(bool value, [CallerMemberName]string key = null)
        {
            LocalSettings.Values[key] = value;
        }

        public static bool EpsBatch
        {
            set => Set(value);
            get => Get();
        }

        public static bool SubjectComplete
        {
            set => Set(value);
            get => Get();
        }

        public static bool OrderByAirTime
        {
            set => Set(value);
            get => Get();
        }

        public static bool UseBangumiData
        {
            set => Set(value);
            get => Get();
        }

        public static bool UseBangumiDataAirSites
        {
            set => Set(value);
            get => UseBangumiData && Get();
        }

        public static bool UseBiliApp
        {
            set => Set(value);
            get => UseBangumiData && UseBangumiDataAirSites && Get();
        }

        public static bool UseBangumiDataAirTime
        {
            set => Set(value);
            get => UseBangumiData && Get();
        }

        public static bool EnableBangumiAirToast
        {
            set => Set(value);
            get => UseBangumiData && UseBangumiDataAirTime && Get();
        }

    }
}
