using System.Threading.Tasks;
using Hephaestus.Model.Core;
using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Nexus.Interfaces
{
    public interface INexusApi : IService
    {
        event NexusApi.HasLoggedIn HasLoggedInEvent;

        string ApiKey();
        Task<GetModsByMd5Result> GetModsByMd5(string md5);
        Task New(GameName gameName, string apiKey = "");
    }
}