using System;
using System.Threading.Tasks;
using ByondHub.Core.Services.ServerService.Models;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;

namespace ByondHub.Core.Services.ServerService.ServerState
{
    public class StoppedServerState : IServerState
    {
        public UpdateResult Update(ServerInstance server, UpdateRequest request)
        {
            return server.Update(request);
        }

        public ServerStartStopResult Start(ServerInstance server, int port)
        {
            return server.Start(port);
        }

        public ServerStartStopResult Stop(ServerInstance server)
        {
            return new ServerStartStopResult
            {
                Error = true,
                ErrorMessage = "Server is not running.",
                Id = server.Build.Id
            };
        }
    }
}
