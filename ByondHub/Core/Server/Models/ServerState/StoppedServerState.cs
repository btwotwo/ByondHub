using System.Threading.Tasks;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;

namespace ByondHub.Core.Server.Models.ServerState
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
            return result;
        }

        public override ServerStartStopResult Start(int port)
        {
            var result = Server.Start(port);
            if (result.Error) return result;

            Server.State = new StartedServerState(Server);
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
