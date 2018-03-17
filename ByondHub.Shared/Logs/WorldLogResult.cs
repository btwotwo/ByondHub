using System.IO;
using ByondHub.Shared.Web;

namespace ByondHub.Shared.Logs
{
    public class WorldLogResult : WebResult
    {
        public Stream LogFileStream { get; set; }
        public string Id { get; set; }
    }
}
