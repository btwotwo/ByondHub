using System.Threading.Tasks;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;

namespace ByondHub.Core.Server.Models.ServerState
{
    public abstract class ServerStateAbstract
    {
        protected ServerInstance Server;

        protected ServerStateAbstract(ServerInstance server)
        {
            Server = server;
            UpdateStatus();
        }

        public abstract UpdateResult Update(UpdateRequest request);
        public abstract ServerStartStopResult Start(int port);
        public abstract ServerStartStopResult Stop();
        public abstract Task UpdatePlayersAsync();
        public abstract void UpdateStatus();
    }
}