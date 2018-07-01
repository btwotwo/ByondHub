using System.Threading.Tasks;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;

namespace ByondHub.Core.Server.ServerState
{
    public class StartedServerState : ServerStateAbstract
    {

        public StartedServerState(ServerInstance server) : base(server)
        {
        }

        public override UpdateResult Update(UpdateRequest request)
        {
            return new UpdateResult
            {
                Id = Server.Build.Id,
                Error = true,
                ErrorMessage = $"Server '{Server.Build.Id}' is running. Please stop it first."
            };
        }

        public override ServerStartStopResult Start(int port)
        {
            return new ServerStartStopResult
            {
                Error = true,
                ErrorMessage = "Server is already started.",
                Id = Server.Build.Id
            };
        }

        public override ServerStartStopResult Stop()
        {
            var result = Server.Stop();
            if (result.Error) return result;

            Server.State = new StoppedServerState(Server);
            return result;
        }

        public override async Task UpdatePlayersAsync()
        {
            await Server.UpdatePlayersAsync();
        }

        public override void UpdateStatus()
        {
            Server.Status.SetStarted();
        }
    }
}
