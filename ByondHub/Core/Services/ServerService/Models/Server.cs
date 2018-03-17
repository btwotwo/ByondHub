using ByondHub.Core.Configuration;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;

namespace ByondHub.Core.Services.ServerService.Models
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
            return _serverInstance.State.Start(_serverInstance, port);
        }

        public ServerStartStopResult Stop()
        {
            return _serverInstance.State.Stop(_serverInstance);
        }

        public UpdateResult Update(UpdateRequest request)
        {
            return _serverInstance.State.Update(_serverInstance, request);
        }
    }
}
