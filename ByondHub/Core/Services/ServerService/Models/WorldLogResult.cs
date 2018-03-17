using System.IO;
using ByondHub.Shared.Web;
using Newtonsoft.Json;

namespace ByondHub.Core.Services.ServerService.Models
{
    public class WorldLogResult : WebResult
    {
        [JsonIgnore]
        public FileStream LogStream { get; set; }
        public string Id { get; set; }
    }
}
