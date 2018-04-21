using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ByondHub.Core.Configuration;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ByondHub.Core.Server.Services
{
    public class ServerService
    {
        private readonly Dictionary<string, Models.Server> _servers;
        private readonly ILogger<ServerService> _logger;

        public ServerService(IConfiguration config, ILogger<ServerService> logger)
        {
            _servers = new Dictionary<string, Models.Server>();
            _logger = logger;

            var builds = config.GetSection("Hub").GetSection("Builds").Get<BuildModel[]>();
            var updater = new ServerUpdater(config, logger);
            foreach (var build in builds)
            {
                _servers.Add(build.Id,
                    new Models.Server(new ServerInstance(build, config["Hub:DreamDaemonPath"], updater,
                        config["Hub:VisibleAddress"], logger)));
            }
        }

        public ServerStartStopResult Start(string serverId, int port)
        {
            try
            {
                var server = _servers[serverId];

                _logger.LogInformation($"Starting server with id {serverId}, port: {port}");
                return server.Start(port);
            }
            catch (KeyNotFoundException)
            {
                return new ServerStartStopResult {Error = true, Id = serverId, ErrorMessage = "Server not found."};
            }
            catch (Exception e)
            {
                return new ServerStartStopResult
                {
                    Error = true,
                    Id = serverId,
                    ErrorMessage = $"Got following exception: {e.Message}"
                };
            }


        }

        public ServerStartStopResult Stop(string serverId)
        {
            try
            {
                var server = _servers[serverId];
                _logger.LogInformation($"Killing server with id {serverId}.");
                return server.Stop();
            }
            catch (KeyNotFoundException)
            {
                return new ServerStartStopResult {Error = true, Id = serverId, ErrorMessage = "Server not found."};
            }
            catch (Exception e)
            {
                return new ServerStartStopResult
                {
                    Error = true,
                    Id = serverId,
                    ErrorMessage = $"Got following exception: {e.Message}"
                };
            }
        }

        public UpdateResult Update(UpdateRequest request)
        {
            try
            {
                var server = _servers[request.Id];
                return server.Update(request);
            }
            catch (KeyNotFoundException)
            {
                return new UpdateResult {Error = true, Id = request.Id, ErrorMessage = "Server not found."};
            }
            catch (Exception e)
            {
                return new UpdateResult
                {
                    Error = true,
                    Id = request.Id,
                    ErrorMessage = $"Got exception: {e.Message}"
                };
            }

        }

        public WorldLogResult GetWorldLog(string serverId)
        {
            try
            {
                var server = _servers[serverId];
                string path = Path.Combine(server.Build.Path, $"{server.Build.ExecutableName}.log");

                bool fileExists = File.Exists(path);
                if (!fileExists)
                {
                    return new WorldLogResult {Error = true, ErrorMessage = "World Log not found.", Id = serverId};
                }

                var stream = new FileStream(path, FileMode.Open);
                return new WorldLogResult {LogFileStream = stream};
            }
            catch (KeyNotFoundException)
            {
                return new WorldLogResult {Error = true, ErrorMessage = "Server not found.", Id = serverId};
            }
            catch (Exception e)
            {
                return new WorldLogResult {Error = true, ErrorMessage = $"Got exception: {e.Message}", Id = serverId};
            }
        }

        public async Task<ServerStatusResult> GetStatus(string serverId)
        {
            try
            {
                var server = _servers[serverId];
                return await server.GetStatusAsync();
            }
            catch (KeyNotFoundException)
            {
                return new ServerStatusResult() {Error = true, ErrorMessage = $"{serverId} not found."};
            }
            catch (Exception e)
            {
                return new ServerStatusResult() {Error = true, ErrorMessage = $"Error: {e}"};
            }
        }
    }
}
