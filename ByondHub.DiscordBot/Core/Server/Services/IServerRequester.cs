using System.Threading.Tasks;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;

namespace ByondHub.DiscordBot.Core.Server.Services
{
    public interface IServerRequester
    {
        Task<ServerStartStopResult> SendStartRequestAsync(string serverId, int port);
        Task<ServerStartStopResult> SendStopRequestAsync(string serverId);
        Task<UpdateResult> SendUpdateRequestAsync(string serverId, string branch, string commitHash);
        Task<WorldLogResult> SendWorldLogRequestAsync(string serverId);
        Task<ServerStatusResult> SendStatusRequestAsync(string serverId);
    }
}
