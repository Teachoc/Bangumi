using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Bangumi.Controls.TitleBar
{
    public sealed partial class TitleBarEx : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
              nameof(Text), typeof(string), typeof(TitleBarEx), new PropertyMetadata(null));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty ImageVisibilityProperty = DependencyProperty.Register(
              nameof(ImageVisibility), typeof(Visibility), typeof(TitleBarEx), new PropertyMetadata(Visibility.Collapsed));
        public Visibility ImageVisibility
        {
            get => (Visibility)GetValue(ImageVisibilityProperty);
            set => SetValue(ImageVisibilityProperty, value);
        }

        public static readonly DependencyProperty LoadingTextProperty = DependencyProperty.Register(
              nameof(LoadingText), typeof(string), typeof(TitleBarEx), new PropertyMetadata(null));
        public string LoadingText
        {
            get => (string)GetValue(LoadingTextProperty);
            set => SetValue(LoadingTextProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
              nameof(IsLoading), typeof(bool), typeof(TitleBarEx), new PropertyMetadata(false));
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty IsBackEnabledProperty = DependencyProperty.Register(
              nameof(IsBackEnabled), typeof(bool), typeof(TitleBarEx), new PropertyMetadata(false));
        public bool IsBackEnabled
        {
            get => (bool)GetValue(IsBackEnabledProperty);
            set
            {
                SetValue(IsBackEnabledProperty, value);
                if (value)
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                    LeftPaddingColumn.Width = new GridLength(48);
                }
                else
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    LeftPaddingColumn.Width = new GridLength(0);
                }
            }
        }

        public TitleBarEx()
        {
            InitializeComponent();
            Window.Current.CoreWindow.Activated += CoreWindow_Activated;
            // 将内容拓展到标题栏
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            if (!coreTitleBar.ExtendViewIntoTitleBar)
            {
                coreTitleBar.ExtendViewIntoTitleBar = true;
            }

            // 设置标题栏系统按钮颜色
            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appTitleBar.ButtonForegroundColor = Colors.White;
            appTitleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x2e, 0xff, 0xff, 0xff);
            appTitleBar.ButtonHoverForegroundColor = Colors.White;
            appTitleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x55, 0xff, 0xff, 0xff);
            appTitleBar.ButtonPressedForegroundColor = Colors.White;
            appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appTitleBar.ButtonInactiveForegroundColor = Colors.White;
        }

        /// <summary>
        /// 在窗口激活状态变化时修改文字颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            SolidColorBrush brush;
            switch (sender.ActivationMode)
            {
                case CoreWindowActivationMode.None:
                    brush = new SolidColorBrush(Color.FromArgb(0xff, 0xf5, 0xe0, 0xe6));
                    break;
                case CoreWindowActivationMode.Deactivated:
                    brush = new SolidColorBrush(Color.FromArgb(0xff, 0xf5, 0xe0, 0xe6));
                    break;
                case CoreWindowActivationMode.ActivatedNotForeground:
                    brush = new SolidColorBrush(Color.FromArgb(0xff, 0xf5, 0xe0, 0xe6));
                    break;
                case CoreWindowActivationMode.ActivatedInForeground:
                    brush = new SolidColorBrush(Colors.White);
                    break;
                default:
                    brush = new SolidColorBrush(Colors.White);
                    break;
            }

            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appTitleBar.ButtonInactiveForegroundColor = brush.Color;

            AppTitleTextBlock.Foreground = brush;
            TitleBarLoadingTextBlock.Foreground = brush;
            TitleBarProgressRing.Foreground = brush;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置标题栏可拖动区域
            Window.Current.SetTitleBar(AppTitleBar);
        }
    }
}
