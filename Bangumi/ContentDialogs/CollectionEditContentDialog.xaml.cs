using Bangumi.Api.Models;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Bangumi.ContentDialogs
{
    public sealed partial class CollectionEditContentDialog : ContentDialog
    {
        public int Rate { get; set; }
        public string Comment { get; set; }
        public bool Privacy { get; set; }
        public CollectionStatusEnum CollectionStatus { get; set; }
        public SubjectTypeEnum SubjectType { get; set; }

        public CollectionEditContentDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Rate = (int)RateSlider.Value;
            Comment = CommentTextBox.Text;
            Privacy = PrivacyCheckBox.IsChecked == true;
        }

        private void StatusRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CollectionStatus = CollectionStatusEnumEx.FromValue(((RadioButton)sender).Tag.ToString());
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            switch (SubjectType)
            {
                case SubjectTypeEnum.Book:
                    WishRadio.Content = "想读";
                    CollectRadio.Content = "读过";
                    DoRadio.Content = "在读";
                    break;
                case SubjectTypeEnum.Anime:
                    WishRadio.Content = "想看";
                    CollectRadio.Content = "看过";
                    DoRadio.Content = "在看";

                    break;
                case SubjectTypeEnum.Music:
                    WishRadio.Content = "想听";
                    CollectRadio.Content = "听过";
                    DoRadio.Content = "在听";

                    break;
                case SubjectTypeEnum.Game:
                    WishRadio.Content = "想玩";
                    CollectRadio.Content = "玩过";
                    DoRadio.Content = "在玩";
                    break;
                case SubjectTypeEnum.Real:
                    WishRadio.Content = "想看";
                    CollectRadio.Content = "看过";
                    DoRadio.Content = "在看";
                    break;
                default:
                    WishRadio.Content = "想看";
                    CollectRadio.Content = "看过";
                    DoRadio.Content = "在看";
                    break;
            }
            if (CollectionStatus != CollectionStatusEnum.No)
            {

                StatusPanel.Children.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == CollectionStatus.GetValue()).IsChecked = true;
            }
        }
    }
}
