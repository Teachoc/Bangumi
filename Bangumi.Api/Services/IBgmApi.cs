using Bangumi.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bangumi.Api.Services
{
    public interface IBgmApi
    {
        Task<List<Watching>> Watching();
        Task<Collection2> Collections(SubjectTypeEnum subjectType);
        Task<Subject> Subject(string subjectId);
        Task<Subject> SubjectEp(string subjectId);
        Task<SubjectStatus2> Status(string subjectId);
        Task<Progress> Progress(string subject_id);
        Task<List<BangumiTimeLine>> Calendar();
        Task<SubjectStatus2> UpdateStatus(string subjectId, 
            CollectionStatusEnum collectionStatusEnum, string comment = "", 
            string rating = "", string privacy = "0");
        Task<bool> UpdateProgress(string ep, EpStatusEnum status);
        Task<bool> UpdateProgressBatch(int ep, EpStatusEnum status, string ep_id);
        Task<SearchResult> Search(string keyWord, string type, int start, int max_results);
    }
}
