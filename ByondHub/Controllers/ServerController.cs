using System;
using ByondHub.Core.Services;
using ByondHub.Shared.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ByondHub.Controllers
{
    [Produces("application/json")]
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
                throw new Exception("Authentication error.");
            }

            _service.Start(serverId, port);
            return Json(new SuccessResponse{Message = $"Started \"{serverId}\" on port \"{port}\"."});
        }

        [HttpPost("stop/{serverId}")]
        public IActionResult Stop(string serverId, [FromForm] string secret)
        {
            if (!string.Equals(secret, _secret))
            {
                throw new Exception("Authentication error.");
            }
            _service.Stop(serverId);
            return Json(new SuccessResponse{Message = $"Stopped server with id \"{serverId}\""});
        }
    }
}