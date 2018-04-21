using System.IO;
using ByondHub.Shared.Core;

namespace ByondHub.Shared.Server
{
    public class WorldLogResult : WebResult
    {
        public Stream LogFileStream { get; set; }
        public string Id { get; set; }
    }
}
