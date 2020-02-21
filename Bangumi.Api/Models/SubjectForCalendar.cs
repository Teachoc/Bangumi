using Bangumi.Api.Common;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Bangumi.Api.Models
{
    public class SubjectForCalendar : SubjectBase, ICollectionStatus
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

        /// <summary>
        /// 排名
        /// </summary>
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("rating")]
        public Rating Rating { get; set; }

        /// <summary>
        /// doing
        /// </summary>
        [JsonProperty("collection")]
        public SubjectCollection Collection { get; set; }


        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectForCalendar s = (SubjectForCalendar)obj;
            return base.Equals(obj) &&
                   Rank == s.Rank &&
                   Rating.EqualsExT(s.Rating) &&
                   Collection.EqualsExT(s.Collection) &&
                   Status == s.Status;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
