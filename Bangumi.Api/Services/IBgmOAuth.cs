using Bangumi.Api.Models;
using System.Threading.Tasks;

namespace Bangumi.Api.Services
{
    public interface IBgmOAuth
    {
        bool IsLogin { get; }
        AccessToken MyToken { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string RedirectUrl { get; }

        Task GetToken(string code);
        Task CheckToken();
        void DeleteUserFiles();
    }
}
