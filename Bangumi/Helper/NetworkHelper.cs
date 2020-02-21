using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Helper
{
    public static class NetworkHelper
    {
        public static bool IsOffline => !Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;

        public static event EventHandler NetworkChanged;

        static NetworkHelper()
        {
            Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper.Instance.NetworkChanged +=
                (sender, e) => DispatcherHelper.ExecuteOnUIThreadAsync(() => NetworkChanged?.Invoke(sender, e));
        }
    }
}
