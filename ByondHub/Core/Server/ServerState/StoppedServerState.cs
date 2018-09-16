using System;
using System.Threading.Tasks;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;

namespace ByondHub.Core.Server.ServerState
{
    public class StoppedServerState : ServerStateAbstract
    {
        public StoppedServerState(ServerInstance server) : base(server)
        {
        }

        public override UpdateResult Update(UpdateRequest request, Func<UpdateRequest, UpdateResult> updateFunc)
        {
            Server.State = new UpdatingServerState(Server);
            var result = updateFunc(request);
            Server.State = new StoppedServerState(Server);
            return result;
        }

        public override ServerStartStopResult Start(ushort port, Func<ushort, ServerStartStopResult> startFunc)
        {
            var result = startFunc(port);
            if (result.Error) return result;

            Server.State = new StartedServerState(Server);
            return result;
        }

        public override ServerStartStopResult Stop(Func<ServerStartStopResult> stopFunc)
        {
            return new ServerStartStopResult
            {
                Error = true,
                ErrorMessage = "Server is not running.",
                Id = Server.Build.Id
            };
        }

        public override Task UpdatePlayersAsync(Func<Task> updatePlayersFunc)
        {
            return Task.CompletedTask;
        }

        public override void UpdateStatus()
        {
            Server.Status.SetStopped();
        }
    }
}
