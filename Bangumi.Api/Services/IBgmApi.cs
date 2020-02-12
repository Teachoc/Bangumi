using Bangumi.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bangumi.Api.Services
{
    public interface IBgmApi
    {
        Task<User> User();
        Task<User> User(string username);
        Task<List<Watching>> Watching();
        Task<CollectionE> Collections(SubjectType subjectType);
        Task<SubjectLarge> Subject(string subjectId);
        Task<SubjectLarge> SubjectEp(string subjectId);
        Task<CollectionStatusE> Status(string subjectId);
        Task<Dictionary<string, CollectionStatus>> Status(IEnumerable<string> subjectIds);
        Task<Progress> Progress(string subject_id);
        Task<List<Calendar>> Calendar();
        Task<CollectionStatusE> UpdateStatus(string subjectId, 
            CollectionStatusType collectionStatusEnum, string comment = "", 
            string rating = "", string privacy = "0");
        Task<bool> UpdateProgress(string ep, EpStatusType status);
        Task<bool> UpdateProgressBatch(int ep, string ep_id);
        Task<SearchResult> Search(string keyWord, string type, int start, int max_results);
    }
}
