
using System;
using ByondHub.Core.Services.ServerService.Models;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;

namespace ByondHub.Core.Services.ServerService.ServerState
{
    public class UpdatingServerState : IServerState
    {
        public UpdateResult Update(ServerInstance server, UpdateRequest request)
        {
            return new UpdateResult
            {
                Error = true,
                ErrorMessage = $"Server with '{server.Build.Id} is already updating.",
                Id = server.Build.Id
            };
        }

        public ServerStartStopResult Start(ServerInstance server, int port)
        {
            return new ServerStartStopResult()
            {
                Error = true,
                ErrorMessage = "Server is updating. Please wait until update process is finished.",
                Id = server.Build.Id
            };
        }

        public ServerStartStopResult Stop(ServerInstance server)
        {
            return new ServerStartStopResult()
            {
                Error = true,
                ErrorMessage = "Server is updating. Wait until update process is finished.",
                Id = server.Build.Id
            };
        }
    }
}
