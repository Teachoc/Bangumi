using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bangumi.Api.Models
{
    public class Ep : INotifyPropertyChanged
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("sort")]
        public double Sort { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("airdate")]
        public string AirDate { get; set; }

        [JsonProperty("comment")]
        public int Comment { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        private string _status { get; set; }

        [JsonProperty("status")]
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Ep e = (Ep)obj;
            return Id == e.Id &&
                   Type == e.Type &&
                   Sort == e.Sort &&
                   Comment == e.Comment &&
                   Url.EqualsExT(e.Url) &&
                   Name.EqualsExT(e.Name) &&
                   NameCn.EqualsExT(e.NameCn) &&
                   Duration.EqualsExT(e.Duration) &&
                   AirDate.EqualsExT(e.AirDate) &&
                   Desc.EqualsExT(e.Desc) &&
                   Status.EqualsExT(e.Status);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
