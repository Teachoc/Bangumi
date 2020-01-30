using Bangumi.Api.Models;
using Bangumi.ViewModels;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Bangumi.ContentDialogs
{
    public sealed partial class CollectionEditContentDialog : ContentDialog, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名</param>
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 检查属性值是否相同。
        /// 仅在不同时设置属性值。
        /// </summary>
        /// <typeparam name="T">属性类型。</typeparam>
        /// <param name="storage">可读写的属性。</param>
        /// <param name="value">属性值。</param>
        /// <param name="propertyName">属性名。可被支持 CallerMemberName 的编译器自动提供。</param>
        /// <returns>值改变则返回 true，未改变返回 false。</returns>
        private bool Set<T>(ref T storage, T value,
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                Set(ref _isLoading, value);
            }
        }

        private int _rate;
        public int Rate
        {
            get => _rate;
            private set
            {
                Set(ref _rate, value);
            }
        }

        private string _comment;
        public string Comment
        {
            get => _comment;
            private set
            {
                Set(ref _comment, value);
            }
        }

        private bool _privacy;
        public bool Privacy
        {
            get => _privacy;
            private set
            {
                Set(ref _privacy, value);
            }
        }

        public CollectionStatusType? CollectionStatus { get; private set; }
        private readonly Task<CollectionStatusE> SubjectStatusTask;

        public CollectionEditContentDialog(SubjectType subjectType, Task<CollectionStatusE> subjectStatusTask)
        {
            this.InitializeComponent();
            switch (subjectType)
            {
                case SubjectType.Book:
                    WishRadio.Content = "想读";
                    CollectRadio.Content = "读过";
                    DoRadio.Content = "在读";
                    break;
                case SubjectType.Music:
                    WishRadio.Content = "想听";
                    CollectRadio.Content = "听过";
                    DoRadio.Content = "在听";
                    break;
                case SubjectType.Game:
                    WishRadio.Content = "想玩";
                    CollectRadio.Content = "玩过";
                    DoRadio.Content = "在玩";
                    break;
                case SubjectType.Anime:
                case SubjectType.Real:
                default:
                    WishRadio.Content = "想看";
                    CollectRadio.Content = "看过";
                    DoRadio.Content = "在看";
                    break;
            }
            SubjectStatusTask = subjectStatusTask;
        }

        private void StatusRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CollectionStatus = CollectionStatusTypeExtension.FromValue(((RadioButton)sender).Tag.ToString());
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            var subjectStatus = await SubjectStatusTask;
            IsLoading = false;
            CollectionStatus = subjectStatus.Status?.Id;
            Rate = subjectStatus.Rating;
            Comment = subjectStatus.Comment;
            Privacy = subjectStatus.Private?.Equals("1") ?? false;

            if (CollectionStatus != null)
                StatusPanel.Children.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == CollectionStatus?.GetValue()).IsChecked = true;
        }
    }
}
