using Bangumi.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bangumi.Api.Services
{
    public interface IBgmCache
    {
        bool IsUpdatedToday { get; set; }
        Task WriteToFile();
        void Delete();
        long GetFileLength();

        internal List<Watching> UpdateWatching(List<Watching> value);
        internal CollectionE UpdateCollections(SubjectType key, CollectionE value);
        internal SubjectLarge UpdateSubject(string key, SubjectLarge value);
        internal SubjectLarge UpdateSubjectEp(string key, SubjectLarge value);
        internal CollectionStatusE UpdateStatus(string key, CollectionStatusE value);
        internal CollectionStatus UpdateStatus(string key, CollectionStatus value);
        internal Progress UpdateProgress(string key, Progress value);
        internal void UpdateProgress(int key, EpStatusType value);
        internal List<Calendar> UpdateCalendar(List<Calendar> value);

        List<Watching> Watching();
        CollectionE Collections(SubjectType key);
        SubjectLarge Subject(string key);
        CollectionStatusE Status(string key);
        Progress Progress(string key);
        List<Calendar> Calendar();

    }
}
