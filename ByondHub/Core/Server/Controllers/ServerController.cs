using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using ByondHub.Core.Server.Services;
using ByondHub.Shared.Updates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ByondHub.Core.Server.Controllers
{
    [Produces("application/json", "application/octet-stream")]
    [Route("api/[controller]")]
    public class ServerController : Controller
    {
        private readonly ServerService _service;
        private readonly string _secret;

        public ServerController(ServerService service, IConfiguration config)
        {
            _service = service;
            _secret = config["Hub:SecretCode"]; //temporary solution
        }

        [HttpPost("start/{serverId}")]
        public IActionResult Start(string serverId, [FromForm]string secret, [FromForm]int port)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new Exception("Authentication error."); // again, all of these are temporary
            }

            var result = _service.Start(serverId, port);
            return Json(result);
        }

        [HttpPost("stop/{serverId}")]
        public IActionResult Stop(string serverId, [FromForm] string secret)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new Exception("Authentication error.");
            }
            var result = _service.Stop(serverId);
            return Json(result);
        }

        [HttpPost("update/{serverId}")]
        public IActionResult Update(string serverId, [FromBody] UpdateRequest request)
        {
            if (!string.Equals(request.SecretKey, _secret))
            {
                throw new Exception("Authentication error.");
            }

            var res = _service.Update(request);
            return Json(res);
        }

        [HttpGet("worldLog/{serverId}")]
        public IActionResult GetWorldLog(string serverId, [FromQuery] string secret)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new AuthenticationException("Authentication error.");
            }

            var result = _service.GetWorldLog(serverId);
            if (result.Error)
            {
                return Json(result);
            }

            return File(result.LogFileStream, "application/octet-stream", $"{serverId}.log");
        }

        public async Task<IActionResult> GetServerStatus(string serverId, [FromQuery] string secret)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new AuthenticationException("Authentication error."); 
            }
            var status = await _service.GetStatus(serverId);
            return Json(status);
        }
    }
}