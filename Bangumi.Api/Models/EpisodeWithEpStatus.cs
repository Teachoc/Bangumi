using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bangumi.Api.Models
{
    public class EpisodeWithEpStatus : Episode, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private EpStatusType _epStatus;
        public EpStatusType EpStatus
        {
            get => _epStatus;
            set
            {
                _epStatus = value;
                OnPropertyChanged();
            }
        }

        public static EpisodeWithEpStatus FromEpisode(Episode ep) => new EpisodeWithEpStatus
        {
            Id = ep.Id,
            Url = ep.Url,
            Type = ep.Type,
            Sort = ep.Sort,
            Name = ep.Name,
            NameCn = ep.NameCn,
            Duration = ep.Duration,
            AirDate = ep.AirDate,
            Comment = ep.Comment,
            Desc = ep.Desc,
            Status = ep.Status,
            EpStatus = EpStatusType.remove
        };
    }
}
