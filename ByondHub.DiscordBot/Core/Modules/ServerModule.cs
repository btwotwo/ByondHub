using System;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Services;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace ByondHub.DiscordBot.Core.Modules
{
    [RequireUserPermission(GuildPermission.BanMembers)]
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
        [Summary("Starts server. Provide id and port.")]
        public async Task StartServerAsync([Summary("Server id")] string id, [Summary("Port to start")] int port)
        {
            string res = await _server.SendStartRequestAsync(id, port);
            await ReplyAsync(res);
        }

        [Command("stop")]
        [Summary("Stops server. Provide id")]
        public async Task StopServer([Summary("Server id")] string id)
        {
            string res = await _server.SendStopRequestAsync(id);
            await ReplyAsync(res);
        }

        [Command("update", RunMode = RunMode.Async)]
        [Summary("Updates server. Provide id.")]

        public async Task UpdateServer([Summary("Server id")] string id)
        {
            await ReplyAsync($"Sent update request for \"{id}\"...");
            var (res, success) = await _server.SendUpdateRequestAsync(id);
            if (!success)
            {
               await ReplyAsync($"Got error: {res}");
               return;
            }
            await ReplyAsync($"Update request is finished: {res}");
        }
    }
}
