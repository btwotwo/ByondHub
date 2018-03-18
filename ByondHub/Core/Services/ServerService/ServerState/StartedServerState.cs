using System;
using ByondHub.Core.Services.ServerService.Models;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;

namespace ByondHub.Core.Services.ServerService.ServerState
{
    public class StartedServerState : IServerState
    {
        public UpdateResult Update(ServerInstance server, UpdateRequest request)
        {
            return new UpdateResult
            {
                Id = server.Build.Id,
                Error = true,
                ErrorMessage = $"Server '{server.Build.Id}' is running. Please stop it first."
            };
        }

        public ServerStartStopResult Start(ServerInstance server, int port)
        {
            return new ServerStartStopResult
            {
                Error = true,
                ErrorMessage = "Server is already started.",
                Id = server.Build.Id
            };
        }

        public ServerStartStopResult Stop(ServerInstance server)
        {
           return server.Stop();
        }
    }
}
