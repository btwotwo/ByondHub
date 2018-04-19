using System;
using System.Threading.Tasks;
using ByondHub.Core.Services.ServerService.Models;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;

namespace ByondHub.Core.Services.ServerService.ServerState
{
    public class StoppedServerState : ServerStateAbstract
    {
        public StoppedServerState(ServerInstance server) : base(server)
        {
        }

        public override UpdateResult Update(UpdateRequest request)
        {
            Server.State = new UpdatingServerState(Server);
            var result = Server.Update(request);
            Server.State = new StoppedServerState(Server);
            return result;
        }

        public override ServerStartStopResult Start(int port)
        {
            var result = Server.Start(port);
            if (result.Error) return result;

            Server.State = new StartedServerState(Server);
            Server.State.UpdateStatus();
            return result;
        }

        public override ServerStartStopResult Stop()
        {
            return new ServerStartStopResult
            {
                Error = true,
                ErrorMessage = "Server is not running.",
                Id = Server.Build.Id
            };
        }

        public override Task UpdatePlayersAsync()
        {
            return Task.CompletedTask;
        }

        public override void UpdateStatus()
        {
            Server.Status.SetStopped();
        }


    }
}
