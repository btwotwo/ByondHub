using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;

namespace ByondHub.Core.Services.ServerService.Models
{
    public class ServerContext
    {
        private readonly ServerInstance _serverInstance;

        public ServerContext(ServerInstance instance)
        {
            _serverInstance = instance;
        }

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
