using Bangumi.Common;
using Bangumi.ViewModels;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DetailPage : Page
    {
        public DetailViewModel ViewModel { get; private set; }

        public DetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            (((Frame.Parent as NavigationView)?.Parent as Grid).Parent as MainPage)?.SelectPlaceholderItem("详情");
            ViewModel = e.Parameter as DetailViewModel;
        }

        private async void ItemsRepeater_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is RelativePanel panel)
            {
                // The URI to launch
                var uriWebPage = new Uri(panel.DataContext.ToString());

                // Launch the URI
                await Launcher.LaunchUriAsync(uriWebPage);
            }
        }

        private void RelativePanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is RelativePanel panel)
            {
                panel.Background = Resources["ListViewItemBackgroundPointerOver"] as SolidColorBrush;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 10);
            }
        }

        private void RelativePanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is RelativePanel panel)
            {
                panel.Background = Resources["ListViewItemBackgroundPressed"] as SolidColorBrush;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 10);
            }
        }

        private void RelativePanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is RelativePanel panel)
            {
                panel.Background = Converters.ConvertBrushFromString("Transparent");
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 10);
            }
        }
    }
}
