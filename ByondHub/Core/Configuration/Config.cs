using System.Collections.Generic;
using Newtonsoft.Json;

namespace ByondHub.Core.Configuration
{
    public class Config
    {
        [JsonProperty("Hub")]
        public Hub Hub { get; set; }

        [JsonProperty("Server")]
        public Server Server { get; set; }
    }

    public class Hub
    {
        [JsonProperty("DreamDaemonPath")]
        public string DreamDaemonPath { get; set; }

        [JsonProperty("DreamMakerPath")]
        public string DreamMakerPath { get; set; }

        [JsonProperty("Builds")]
        public List<BuildModel> Builds { get; set; }

        [JsonProperty("SecretCode")]
        public string SecretCode { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }
    }

    public class BuildModel
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Path")]
        public string Path { get; set; }

        [JsonProperty("ExecutableName")]
        public string ExecutableName { get; set; }

        [JsonProperty("RepositoryUsername")]
        public string RepositoryUsername { get; set; }

        [JsonProperty("RepositoryEmail")]
        public string RepositoryEmail { get; set; }

        [JsonProperty("RepositoryPassword")]
        public string RepositoryPassword { get; set; }

        [JsonProperty("LogPath")]
        public string LogPath { get; set; }
    }

    public class Server
    {
        [JsonProperty("Port")]
        public long Port { get; set; }
    }
}
