using System;
using System.Threading.Tasks;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;

namespace ByondHub.Core.Server.ServerState
{
    public class UpdatingServerState : ServerStateAbstract
    {
        public UpdatingServerState(ServerInstance server) : base(server)
        {
        }

        public override UpdateResult Update(UpdateRequest request, Func<UpdateRequest, UpdateResult> updateFunc)
        {
            return new UpdateResult
            {
                Error = true,
                ErrorMessage = $"Server with '{Server.Build.Id} is already updating.",
                Id = Server.Build.Id
            };
        }

        public override ServerStartStopResult Start(ushort port, Func<ushort, ServerStartStopResult> startFunc)
        {
            return new ServerStartStopResult()
            {
                Error = true,
                ErrorMessage = "Server is updating. Please wait until update process is finished.",
                Id = Server.Build.Id
            };
        }

        public override ServerStartStopResult Stop(Func<ServerStartStopResult> stopFunc)
        {
            return new ServerStartStopResult()
            {
                Error = true,
                ErrorMessage = "Server is updating. Wait until update process is finished.",
                Id = Server.Build.Id
            };
        }

        public override Task UpdatePlayersAsync(Func<Task> updatePlayersFunc)
        {
            return Task.CompletedTask;
        }

        public override void UpdateStatus()
        {
            Server.Status.SetUpdating();
        }
    }
}
