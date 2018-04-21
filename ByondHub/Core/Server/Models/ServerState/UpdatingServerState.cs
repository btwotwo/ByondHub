using System.Threading.Tasks;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;

namespace ByondHub.Core.Server.Models.ServerState
{
    public class UpdatingServerState : ServerStateAbstract
    {
        public UpdatingServerState(ServerInstance server) : base(server)
        {
        }

        public override UpdateResult Update(UpdateRequest request)
        {
            return new UpdateResult
            {
                Error = true,
                ErrorMessage = $"Server with '{Server.Build.Id} is already updating.",
                Id = Server.Build.Id
            };
        }

        public override ServerStartStopResult Start(int port)
        {
            return new ServerStartStopResult()
            {
                Error = true,
                ErrorMessage = "Server is updating. Please wait until update process is finished.",
                Id = Server.Build.Id
            };
        }

        public override ServerStartStopResult Stop()
        {
            return new ServerStartStopResult()
            {
                Error = true,
                ErrorMessage = "Server is updating. Wait until update process is finished.",
                Id = Server.Build.Id
            };
        }

        public override Task UpdatePlayersAsync()
        {
            return Task.CompletedTask;
        }

        public override void UpdateStatus()
        {
            Server.Status.SetUpdating();
        }
    }
}
