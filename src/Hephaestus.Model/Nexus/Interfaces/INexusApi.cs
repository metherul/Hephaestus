using System.Threading.Tasks;
using Hephaestus.Model.Core;
using Hephaestus.Model.Core.Interfaces;
using Hephaestus.Model.Interfaces;
using Hephaestus.Model.Transcompiler;

namespace Hephaestus.Model.Nexus.Interfaces
{
    public interface INexusApi : IService
    {
        event NexusApi.HasLoggedIn HasLoggedInEvent;

        string ApiKey();
        int RemainingDailyRequests { get; set; }
        Task<GetModsByMd5Result> GetModsByMd5(IntermediaryModObject mod);
        Task New(GameName gameName, string apiKey = "");
    }
}