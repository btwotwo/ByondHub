using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Threading.Tasks;
using ByondHub.Core.Configuration;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ByondHub.Core.Server.Controllers
{
    [Produces("application/json", "application/octet-stream")]
    [Route("api/[controller]")]
    public class ServerController : Controller
    {
        private readonly string _secret;
        private readonly Dictionary<string, Models.Server> _servers;
        private readonly ILogger _logger;

        public ServerController(IConfiguration config, ILogger logger)
        {
            _secret = config["Hub:SecretCode"]; //temporary solution
            _servers = new Dictionary<string, Models.Server>();
            _logger = logger;

            var builds = config.GetSection("Hub").GetSection("Builds").Get<BuildModel[]>();
            foreach (var build in builds)
            {
                _servers.Add(build.Id,
                    new Models.Server(new ServerInstance(build, config,
                        config["Hub:Address"], logger)));
            }
        }

        [HttpPost("start/{serverId}")]
        public IActionResult Start(string serverId, [FromForm] string secret, [FromForm] int port)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new Exception("Authentication error."); // again, all of these are temporary
            }

            try
            {
                var server = _servers[serverId];

                _logger.LogInformation($"Starting server with id {serverId}, port: {port}");
                return Json(server.Start(port));
            }
            catch (KeyNotFoundException)
            {
                return Json(new ServerStartStopResult
                {
                    Error = true,
                    Id = serverId,
                    ErrorMessage = "Server not found."
                });
            }
        }

        [HttpPost("stop/{serverId}")]
        public IActionResult Stop(string serverId, [FromForm] string secret)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new Exception("Authentication error.");
            }

            ServerStartStopResult result;

            try
            {
                var server = _servers[serverId];
                _logger.LogInformation($"Killing server with id {serverId}.");
                result = server.Stop();
            }
            catch (KeyNotFoundException)
            {
                result = new ServerStartStopResult { Error = true, Id = serverId, ErrorMessage = "Server not found." };
            }

            return Json(result);
        }

        [HttpPost("update/{serverId}")]
        public IActionResult Update(string serverId, [FromBody] UpdateRequest request)
        {
            if (!string.Equals(request.SecretKey, _secret))
            {
                throw new Exception("Authentication error.");
            }

            UpdateResult res;
            try
            {
                var server = _servers[request.Id];
                res = server.Update(request);
            }
            catch (KeyNotFoundException)
            {
                res = new UpdateResult { Error = true, Id = request.Id, ErrorMessage = "Server not found." };
            }
            return Json(res);
        }

        [HttpGet("worldLog/{serverId}")]
        public IActionResult GetWorldLog(string serverId, [FromQuery] string secret)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new AuthenticationException("Authentication error.");
            }

            WorldLogResult result;

            try
            {
                var server = _servers[serverId];
                string path = Path.Combine(server.Build.Path, $"{server.Build.ExecutableName}.log");

                bool fileExists = System.IO.File.Exists(path);
                if (!fileExists)
                {
                    return Json(new WorldLogResult { Error = true, ErrorMessage = "World Log not found.", Id = serverId });
                }

                var stream = new FileStream(path, FileMode.Open);
                result = new WorldLogResult { LogFileStream = stream };
            }
            catch (KeyNotFoundException)
            {
                result = new WorldLogResult { Error = true, ErrorMessage = "Server not found.", Id = serverId };
            }

            if (result.Error)
            {
                return Json(result);
            }
            return File(result.LogFileStream, "application/octet-stream", $"{serverId}.log");
        }

        [HttpGet("status/{serverId}")]
        public async Task<IActionResult> GetServerStatus(string serverId)
        {
            ServerStatusResult result;
            try
            {
                var server = _servers[serverId];
                result = await server.GetStatusAsync();
            }
            catch (KeyNotFoundException)
            {
                result = new ServerStatusResult() { Error = true, ErrorMessage = $"{serverId} not found." };
            }
            return Json(result);
        }
    }
}