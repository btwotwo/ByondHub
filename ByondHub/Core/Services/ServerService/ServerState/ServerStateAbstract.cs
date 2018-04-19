using System.Threading.Tasks;
using ByondHub.Core.Services.ServerService.Models;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;

namespace ByondHub.Core.Services.ServerService.ServerState
{
    public abstract class ServerStateAbstract
    {
        protected ServerInstance Server;

        protected ServerStateAbstract(ServerInstance server)
        {
            Server = server;
        }

        public abstract UpdateResult Update(UpdateRequest request);
        public abstract ServerStartStopResult Start(int port);
        public abstract ServerStartStopResult Stop();
        public abstract Task UpdatePlayersAsync();
        public abstract void UpdateStatus();
    }
}