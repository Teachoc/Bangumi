using System.ComponentModel;

namespace Bangumi.Api.Models
{
    public class SubjectForSearch : SubjectBase, ICollectionStatus
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名</param>
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private CollectionStatusType? _status;
        public CollectionStatusType? Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectForSearch s = (SubjectForSearch)obj;
            return base.Equals(obj) &&
                   Status == s.Status;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
