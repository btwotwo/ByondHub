using System.Threading.Tasks;
using ByondHub.Core.Configuration;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;

namespace ByondHub.Core.Server.Models
{
    public class Server
    {
        private readonly ServerInstance _serverInstance;

        public Server(ServerInstance instance)
        {
            _serverInstance = instance;
        }

        public BuildModel Build => _serverInstance.Build;

        public ServerStartStopResult Start(int port)
        {
            return _serverInstance.State.Start(port);
        }

        public ServerStartStopResult Stop()
        {
            return _serverInstance.State.Stop();
        }

        public UpdateResult Update(UpdateRequest request)
        {
            return _serverInstance.State.Update(request);
        }

        public async Task<ServerStatusResult> GetStatusAsync()
        {
            await _serverInstance.State.UpdatePlayersAsync();
            return _serverInstance.Status;
        }
    }
}
