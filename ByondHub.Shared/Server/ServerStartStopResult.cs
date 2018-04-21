
using ByondHub.Shared.Core;

namespace ByondHub.Shared.Server
{
    public class ServerStartStopResult : WebResult
    {
        public string Id { get; set; }
        public int Port { get; set; }
    }
}
