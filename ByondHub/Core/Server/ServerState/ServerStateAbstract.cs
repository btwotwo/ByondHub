using System;
using System.Threading.Tasks;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;

namespace ByondHub.Core.Server.ServerState
{
    public abstract class ServerStateAbstract
    {
        protected ServerInstance Server;

        protected ServerStateAbstract(ServerInstance server)
        {
            Server = server;
            UpdateStatus();
        }

        public abstract UpdateResult Update(UpdateRequest request, Func<UpdateRequest, UpdateResult> updateFunc);
        public abstract ServerStartStopResult Start(int port, Func<int, ServerStartStopResult> startFunc);
        public abstract ServerStartStopResult Stop(Func<ServerStartStopResult> stopFunc);
        public abstract Task UpdatePlayersAsync(Func<Task> updatePlayersFunc);
        public abstract void UpdateStatus();
    }
}