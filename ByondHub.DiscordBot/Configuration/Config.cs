using System.Collections.Generic;
using Newtonsoft.Json;

namespace ByondHub.DiscordBot.Configuration
{
    public class Config
    {
        [JsonProperty("Bot")]
        public Bot Bot { get; set; }
    }

    public class Bot
    {
        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; set; }

        [JsonProperty("Backend")]
        public Backend Backend { get; set; }

        [JsonProperty("AdministratorRoles")]
        public List<long> AdministratorRoles { get; set; }
    }

    public class Backend
    {
        [JsonProperty("Host")]
        public string Host { get; set; }

        [JsonProperty("Port")]
        public string Port { get; set; }

        [JsonProperty("SecretCode")]
        public string SecretCode { get; set; }
    }
}