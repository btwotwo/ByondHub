using System;
using System.Collections.Generic;
using System.Linq;
using ByondHub.Core.Configuration;
using ByondHub.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ByondHub.Core.Services
{
    public class ServerService
    {
        private readonly Dictionary<string, ServerInstance> _servers;
        private readonly IConfiguration _config;
        private readonly ILogger<ServerService> _logger;
        private readonly BuildModel[] _builds;

        public ServerService(IConfiguration config, ILogger<ServerService> logger)
        {
            _servers = new Dictionary<string, ServerInstance>();
            _config = config;
            _logger = logger;
            _builds = _config.GetSection("Hub").GetSection("Builds").Get<BuildModel[]>();
        }

        public void Start(string serverId, int port)
        {
            if (_servers.ContainsKey(serverId))
            {
                throw new Exception($"Server with id {serverId} is already started.");
            }
            var build = _builds.FirstOrDefault(x => x.Id == serverId);
            if (build == null)
            {
                throw new Exception($"Server with id {serverId} is not found.");
            }

            var server = new ServerInstance(build, _config["Hub:DreamDaemonPath"], port);
            _servers.Add(serverId, server);
            server.Start();
            _logger.LogInformation($"Starting server with id {serverId}, port: {port}");
        }

        public void Stop(string serverId)
        {
            if (!_servers.ContainsKey(serverId))
            {
                throw new Exception($"Server with id {serverId} is not started");
            }

            var server = _servers[serverId];
            server.Stop();

            _servers.Remove(serverId);
            _logger.LogInformation($"Killed server with id {serverId}.");
        }
    }
}
