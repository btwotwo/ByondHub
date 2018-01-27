using System;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Services;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace ByondHub.DiscordBot.Core.Modules
{
    [Group("server")]
    public class ServerModule : ModuleBase<SocketCommandContext>
    {
        private readonly ServerService _server;
        private readonly ILogger _logger;

        public ServerModule(ServerService service, ILogger logger)
        {
            _server = service;
            _logger = logger;
        }

        [Command("start")]
        [Summary("Starts server. Provide Id and port.")]
        public async Task StartServerAsync([Summary("Server Id")] string id, [Summary("Port to start")] int port)
        {
            string res = await _server.SendStartRequestAsync(id, port);
            await ReplyAsync(res);
        }

        [Command("stop")]
        [Summary("Stops server. Provide Id")]
        public async Task StopServer([Summary("Server Id")] string id)
        {
            string res = await _server.SendStopRequestAsync(id);
            await ReplyAsync(res);
        }
    }
}
