using System.Threading.Tasks;
using ByondHub.Core.Services.ServerService.Models;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;

namespace ByondHub.Core.Services.ServerService.ServerState
{
    public interface IServerState
    {
        UpdateResult Update(ServerInstance server, UpdateRequest request);
        ServerStartStopResult Start(ServerInstance server, int port);
        ServerStartStopResult Stop(ServerInstance server);
    }
}