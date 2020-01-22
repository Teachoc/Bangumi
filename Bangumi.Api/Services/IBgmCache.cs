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
        internal Collection2 UpdateCollections(SubjectTypeEnum key, Collection2 value);
        internal Subject UpdateSubject(string key, Subject value);
        internal Subject UpdateSubjectEp(string key, Subject value);
        internal SubjectStatus2 UpdateStatus(string key, SubjectStatus2 value);
        internal Progress UpdateProgress(string key, Progress value);
        internal void UpdateProgress(int key, EpStatusEnum value);
        internal List<BangumiTimeLine> UpdateCalendar(List<BangumiTimeLine> value);

        List<Watching> Watching();
        Collection2 Collections(SubjectTypeEnum key);
        Subject Subject(string key);
        SubjectStatus2 Status(string key);
        Progress Progress(string key);
        List<BangumiTimeLine> Calendar();

    }
}
