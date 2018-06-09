using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Threading.Tasks;
using ByondHub.Core.Configuration;
using ByondHub.Core.Server.Services;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ByondHub.Core.Server.Controllers
{
    [Produces("application/json", "application/octet-stream")]
    [Route("api/[controller]")]
    public class ServerController : Controller
    {
        private readonly string _secret;
        private readonly ILogger<ServerController> _logger;
        private readonly ServerFactory _servers;

        public ServerController(IOptions<Config> options, ILogger<ServerController> logger, ServerFactory servers)
        {
            var config = options.Value;
            _secret = config.Hub.SecretCode; //temporary solution
            _logger = logger;
            _servers = servers;
        }


        [HttpPost("start/{serverId}")]
        public IActionResult Start(string serverId, [FromForm] string secret, [FromForm] int port)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new Exception("Authentication error."); // again, all of these are temporary
            }
            var server = _servers.GetServer(serverId);
            if (server == null)
            {
                return Json(new ServerStartStopResult
                {
                    Error = true,
                    Id = serverId,
                    ErrorMessage = "Server not found."
                });
            }

            _logger.LogInformation($"Starting server with id {serverId}, port: {port}");
            return Json(server.Start(port));

        }

        [HttpPost("stop/{serverId}")]
        public IActionResult Stop(string serverId, [FromForm] string secret)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new Exception("Authentication error.");
            }
            var server = _servers.GetServer(serverId);

            if (server == null)
            {
                return Json(
                    new ServerStartStopResult() {Error = true, Id = serverId, ErrorMessage = "Server not found"});
            }

            _logger.LogInformation($"Killing server with id {serverId}.");
            return Json(server.Stop());

        }

        [HttpPost("update/{serverId}")]
        public IActionResult Update(string serverId, [FromBody] UpdateRequest request)
        {
            if (!string.Equals(request.SecretKey, _secret))
            {
                throw new Exception("Authentication error.");
            }
            var server = _servers.GetServer(serverId);
            if (server == null)
            {
                return Json(new UpdateResult { Error = true, Id = request.Id, ErrorMessage = "Server not found." });
            }
            _logger.LogInformation($"Updating server {serverId}");
            return Json(server.Update(request));
        }

        [HttpGet("worldLog/{serverId}")]
        public IActionResult GetWorldLog(string serverId, [FromQuery] string secret)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new AuthenticationException("Authentication error.");
            }


            var server = _servers.GetServer(serverId);

            if (server == null)
            {
                return Json(new WorldLogResult { Error = true, ErrorMessage = "Server not found", Id = serverId });
            }

            string path = Path.Combine(server.Build.Path, $"{server.Build.ExecutableName}.log");

            bool fileExists = System.IO.File.Exists(path);
            if (!fileExists)
            {
                return Json(new WorldLogResult { Error = true, ErrorMessage = "World Log not found.", Id = serverId });
            }

            var stream = new FileStream(path, FileMode.Open);
            var result = new WorldLogResult { LogFileStream = stream };

            if (result.Error)
            {
                return Json(result);
            }
            return File(result.LogFileStream, "application/octet-stream", $"{serverId}.log");
        }

        [HttpGet("status/{serverId}")]
        public async Task<IActionResult> GetServerStatus(string serverId)
        {
            var server = _servers.GetServer(serverId);

            return server == null
                ? Json(new ServerStatusResult() {Error = true, ErrorMessage = $"{serverId} not found."})
                : Json(await server.GetStatusAsync());
        }
    }
}